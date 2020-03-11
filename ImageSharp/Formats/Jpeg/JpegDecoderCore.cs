// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Common.Helpers;
using SixLabors.ImageSharp.Formats.Jpeg.Components;
using SixLabors.ImageSharp.Formats.Jpeg.Components.Decoder;
using SixLabors.ImageSharp.IO;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Formats.Jpeg
{
    /// <summary>
    /// Performs the jpeg decoding operation.
    /// Originally ported from <see href="https://github.com/mozilla/pdf.js/blob/master/src/core/jpg.js"/>
    /// with additional fixes for both performance and common encoding errors.
    /// </summary>
    internal sealed class JpegDecoderCore : IRawJpegData
    {
        /// <summary>
        /// The only supported precision
        /// </summary>
        private readonly int[] supportedPrecisions = { 8, 12 };

        /// <summary>
        /// The global configuration
        /// </summary>
        private readonly Configuration configuration;

        /// <summary>
        /// The buffer used to temporarily store bytes read from the stream.
        /// </summary>
        private readonly byte[] temp = new byte[2 * 16 * 4];

        /// <summary>
        /// The buffer used to read markers from the stream.
        /// </summary>
        private readonly byte[] markerBuffer = new byte[2];

        /// <summary>
        /// The DC Huffman tables
        /// </summary>
        private HuffmanTable[] dcHuffmanTables;

        /// <summary>
        /// The AC Huffman tables
        /// </summary>
        private HuffmanTable[] acHuffmanTables;

        /// <summary>
        /// The reset interval determined by RST markers
        /// </summary>
        private ushort resetInterval;

        /// <summary>
        /// Whether the image has an EXIF marker
        /// </summary>
        private bool isExif;

        /// <summary>
        /// Contains exif data
        /// </summary>
        private byte[] exifData;

        /// <summary>
        /// Whether the image has an ICC marker
        /// </summary>
        private bool isIcc;

        /// <summary>
        /// Contains ICC data
        /// </summary>
        private byte[] iccData;

        /// <summary>
        /// Contains information about the JFIF marker
        /// </summary>
        private JFifMarker jFif;

        /// <summary>
        /// Contains information about the Adobe marker
        /// </summary>
        private AdobeMarker adobe;

        /// <summary>
        /// Initializes a new instance of the <see cref="JpegDecoderCore" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        public JpegDecoderCore(Configuration configuration, IJpegDecoderOptions options)
        {
            this.configuration = configuration ?? Configuration.Default;
            this.IgnoreMetadata = options.IgnoreMetadata;
        }

        /// <summary>
        /// Gets the frame
        /// </summary>
        public JpegFrame Frame { get; private set; }

        /// <inheritdoc/>
        public Size ImageSizeInPixels { get; private set; }

        /// <summary>
        /// Gets the number of MCU blocks in the image as <see cref="Size"/>.
        /// </summary>
        public Size ImageSizeInMCU { get; private set; }

        /// <summary>
        /// Gets the image width
        /// </summary>
        public int ImageWidth => this.ImageSizeInPixels.Width;

        /// <summary>
        /// Gets the image height
        /// </summary>
        public int ImageHeight => this.ImageSizeInPixels.Height;

        /// <summary>
        /// Gets the color depth, in number of bits per pixel.
        /// </summary>
        public int BitsPerPixel => this.ComponentCount * this.Frame.Precision;

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        public DoubleBufferedStreamReader InputStream { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the metadata should be ignored when the image is being decoded.
        /// </summary>
        public bool IgnoreMetadata { get; }

        /// <summary>
        /// Gets the <see cref="ImageMetadata"/> decoded by this decoder instance.
        /// </summary>
        public ImageMetadata Metadata { get; private set; }

        /// <inheritdoc/>
        public int ComponentCount { get; private set; }

        /// <inheritdoc/>
        public JpegColorSpace ColorSpace { get; private set; }

        /// <inheritdoc/>
        public int Precision { get; private set; }

        /// <summary>
        /// Gets the components.
        /// </summary>
        public JpegComponent[] Components => this.Frame.Components;

        /// <inheritdoc/>
        IJpegComponent[] IRawJpegData.Components => this.Components;

        /// <inheritdoc/>
        public Block8x8F[] QuantizationTables { get; private set; }

        /// <summary>
        /// Finds the next file marker within the byte stream.
        /// </summary>
        /// <param name="marker">The buffer to read file markers to</param>
        /// <param name="stream">The input stream</param>
        /// <returns>The <see cref="JpegFileMarker"/></returns>
        public static JpegFileMarker FindNextFileMarker(byte[] marker, DoubleBufferedStreamReader stream)
        {
            int value = stream.Read(marker, 0, 2);

            if (value == 0)
            {
                return new JpegFileMarker(JpegConstants.Markers.EOI, stream.Length - 2);
            }

            if (marker[0] == JpegConstants.Markers.XFF)
            {
                // According to Section B.1.1.2:
                // "Any marker may optionally be preceded by any number of fill bytes, which are bytes assigned code 0xFF."
                int m = marker[1];
                while (m == JpegConstants.Markers.XFF)
                {
                    int suffix = stream.ReadByte();
                    if (suffix == -1)
                    {
                        return new JpegFileMarker(JpegConstants.Markers.EOI, stream.Length - 2);
                    }

                    m = suffix;
                }

                return new JpegFileMarker((byte)m, stream.Position - 2);
            }

            return new JpegFileMarker(marker[1], stream.Position - 2, true);
        }

        /// <summary>
        /// Decodes the image from the specified <see cref="Stream"/>  and sets the data to image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="stream">The stream, where the image should be.</param>
        /// <returns>The decoded image.</returns>
        public Image<TPixel> Decode<TPixel>(Stream stream)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            this.ParseStream(stream);
            this.InitExifProfile();
            this.InitIccProfile();
            this.InitDerivedMetadataProperties();
            return this.PostProcessIntoImage<TPixel>();
        }

        /// <summary>
        /// Reads the raw image information from the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        public IImageInfo Identify(Stream stream)
        {
            this.ParseStream(stream, true);
            this.InitExifProfile();
            this.InitIccProfile();
            this.InitDerivedMetadataProperties();

            return new ImageInfo(new PixelTypeInfo(this.BitsPerPixel), this.ImageWidth, this.ImageHeight, this.Metadata);
        }

        /// <summary>
        /// Parses the input stream for file markers
        /// </summary>
        /// <param name="stream">The input stream</param>
        /// <param name="metadataOnly">Whether to decode metadata only.</param>
        public void ParseStream(Stream stream, bool metadataOnly = false)
        {
            this.Metadata = new ImageMetadata();
            this.InputStream = new DoubleBufferedStreamReader(this.configuration.MemoryAllocator, stream);

            // Check for the Start Of Image marker.
            this.InputStream.Read(this.markerBuffer, 0, 2);
            var fileMarker = new JpegFileMarker(this.markerBuffer[1], 0);
            if (fileMarker.Marker != JpegConstants.Markers.SOI)
            {
                JpegThrowHelper.ThrowImageFormatException("Missing SOI marker.");
            }

            this.InputStream.Read(this.markerBuffer, 0, 2);
            byte marker = this.markerBuffer[1];
            fileMarker = new JpegFileMarker(marker, (int)this.InputStream.Position - 2);
            this.QuantizationTables = new Block8x8F[4];

            // Only assign what we need
            if (!metadataOnly)
            {
                const int maxTables = 4;
                this.dcHuffmanTables = new HuffmanTable[maxTables];
                this.acHuffmanTables = new HuffmanTable[maxTables];
            }

            // Break only when we discover a valid EOI marker.
            // https://github.com/SixLabors/ImageSharp/issues/695
            while (fileMarker.Marker != JpegConstants.Markers.EOI
                || (fileMarker.Marker == JpegConstants.Markers.EOI && fileMarker.Invalid))
            {
                if (!fileMarker.Invalid)
                {
                    // Get the marker length
                    int remaining = this.ReadUint16() - 2;

                    switch (fileMarker.Marker)
                    {
                        case JpegConstants.Markers.SOF0:
                        case JpegConstants.Markers.SOF1:
                        case JpegConstants.Markers.SOF2:
                            this.ProcessStartOfFrameMarker(remaining, fileMarker, metadataOnly);
                            break;

                        case JpegConstants.Markers.SOS:
                            if (!metadataOnly)
                            {
                                this.ProcessStartOfScanMarker();
                                break;
                            }
                            else
                            {
                                // It's highly unlikely that APPn related data will be found after the SOS marker
                                // We should have gathered everything we need by now.
                                return;
                            }

                        case JpegConstants.Markers.DHT:

                            if (metadataOnly)
                            {
                                this.InputStream.Skip(remaining);
                            }
                            else
                            {
                                this.ProcessDefineHuffmanTablesMarker(remaining);
                            }

                            break;

                        case JpegConstants.Markers.DQT:
                            this.ProcessDefineQuantizationTablesMarker(remaining);
                            break;

                        case JpegConstants.Markers.DRI:
                            if (metadataOnly)
                            {
                                this.InputStream.Skip(remaining);
                            }
                            else
                            {
                                this.ProcessDefineRestartIntervalMarker(remaining);
                            }

                            break;

                        case JpegConstants.Markers.APP0:
                            this.ProcessApplicationHeaderMarker(remaining);
                            break;

                        case JpegConstants.Markers.APP1:
                            this.ProcessApp1Marker(remaining);
                            break;

                        case JpegConstants.Markers.APP2:
                            this.ProcessApp2Marker(remaining);
                            break;

                        case JpegConstants.Markers.APP3:
                        case JpegConstants.Markers.APP4:
                        case JpegConstants.Markers.APP5:
                        case JpegConstants.Markers.APP6:
                        case JpegConstants.Markers.APP7:
                        case JpegConstants.Markers.APP8:
                        case JpegConstants.Markers.APP9:
                        case JpegConstants.Markers.APP10:
                        case JpegConstants.Markers.APP11:
                        case JpegConstants.Markers.APP12:
                        case JpegConstants.Markers.APP13:
                            this.InputStream.Skip(remaining);
                            break;

                        case JpegConstants.Markers.APP14:
                            this.ProcessApp14Marker(remaining);
                            break;

                        case JpegConstants.Markers.APP15:
                        case JpegConstants.Markers.COM:
                            this.InputStream.Skip(remaining);
                            break;
                    }
                }

                // Read on.
                fileMarker = FindNextFileMarker(this.markerBuffer, this.InputStream);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.InputStream?.Dispose();
            this.Frame?.Dispose();

            // Set large fields to null.
            this.InputStream = null;
            this.Frame = null;
            this.dcHuffmanTables = null;
            this.acHuffmanTables = null;
        }

        /// <summary>
        /// Returns the correct colorspace based on the image component count
        /// </summary>
        /// <returns>The <see cref="JpegColorSpace"/></returns>
        private JpegColorSpace DeduceJpegColorSpace()
        {
            if (this.ComponentCount == 1)
            {
                return JpegColorSpace.Grayscale;
            }

            if (this.ComponentCount == 3)
            {
                if (!this.adobe.Equals(default) && this.adobe.ColorTransform == JpegConstants.Adobe.ColorTransformUnknown)
                {
                    return JpegColorSpace.RGB;
                }

                // Some images are poorly encoded and contain incorrect colorspace transform metadata.
                // We ignore that and always fall back to the default colorspace.
                return JpegColorSpace.YCbCr;
            }

            if (this.ComponentCount == 4)
            {
                return this.adobe.ColorTransform == JpegConstants.Adobe.ColorTransformYcck
                    ? JpegColorSpace.Ycck
                    : JpegColorSpace.Cmyk;
            }

            JpegThrowHelper.ThrowImageFormatException($"Unsupported color mode. Supported component counts 1, 3, and 4; found {this.ComponentCount}");
            return default;
        }

        /// <summary>
        /// Initializes the EXIF profile.
        /// </summary>
        private void InitExifProfile()
        {
            if (this.isExif)
            {
                this.Metadata.ExifProfile = new ExifProfile(this.exifData);
            }
        }

        /// <summary>
        /// Initializes the ICC profile.
        /// </summary>
        private void InitIccProfile()
        {
            if (this.isIcc)
            {
                var profile = new IccProfile(this.iccData);
                if (profile.CheckIsValid())
                {
                    this.Metadata.IccProfile = profile;
                }
            }
        }

        /// <summary>
        /// Assigns derived metadata properties to <see cref="Metadata"/>, eg. horizontal and vertical resolution if it has a JFIF header.
        /// </summary>
        private void InitDerivedMetadataProperties()
        {
            if (this.jFif.XDensity > 0 && this.jFif.YDensity > 0)
            {
                this.Metadata.HorizontalResolution = this.jFif.XDensity;
                this.Metadata.VerticalResolution = this.jFif.YDensity;
                this.Metadata.ResolutionUnits = this.jFif.DensityUnits;
            }
            else if (this.isExif)
            {
                double horizontalValue = this.GetExifResolutionValue(ExifTag.XResolution);
                double verticalValue = this.GetExifResolutionValue(ExifTag.YResolution);

                if (horizontalValue > 0 && verticalValue > 0)
                {
                    this.Metadata.HorizontalResolution = horizontalValue;
                    this.Metadata.VerticalResolution = verticalValue;
                    this.Metadata.ResolutionUnits = UnitConverter.ExifProfileToResolutionUnit(this.Metadata.ExifProfile);
                }
            }
        }

        private double GetExifResolutionValue(ExifTag<Rational> tag)
        {
            IExifValue<Rational> resolution = this.Metadata.ExifProfile.GetValue(tag);

            return resolution is null ? 0 : resolution.Value.ToDouble();
        }

        /// <summary>
        /// Extends the profile with additional data.
        /// </summary>
        /// <param name="profile">The profile data array.</param>
        /// <param name="extension">The array containing addition profile data.</param>
        private void ExtendProfile(ref byte[] profile, byte[] extension)
        {
            int currentLength = profile.Length;

            Array.Resize(ref profile, currentLength + extension.Length);
            Buffer.BlockCopy(extension, 0, profile, currentLength, extension.Length);
        }

        /// <summary>
        /// Processes the application header containing the JFIF identifier plus extra data.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApplicationHeaderMarker(int remaining)
        {
            // We can only decode JFif identifiers.
            if (remaining < JFifMarker.Length)
            {
                // Skip the application header length
                this.InputStream.Skip(remaining);
                return;
            }

            this.InputStream.Read(this.temp, 0, JFifMarker.Length);
            remaining -= JFifMarker.Length;

            JFifMarker.TryParse(this.temp, out this.jFif);

            // TODO: thumbnail
            if (remaining > 0)
            {
                this.InputStream.Skip(remaining);
            }
        }

        /// <summary>
        /// Processes the App1 marker retrieving any stored metadata
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApp1Marker(int remaining)
        {
            const int Exif00 = 6;
            if (remaining < Exif00 || this.IgnoreMetadata)
            {
                // Skip the application header length
                this.InputStream.Skip(remaining);
                return;
            }

            var profile = new byte[remaining];
            this.InputStream.Read(profile, 0, remaining);

            if (ProfileResolver.IsProfile(profile, ProfileResolver.ExifMarker))
            {
                this.isExif = true;
                if (this.exifData is null)
                {
                    // The first 6 bytes (Exif00) will be skipped, because this is Jpeg specific
                    this.exifData = profile.AsSpan(Exif00).ToArray();
                }
                else
                {
                    // If the EXIF information exceeds 64K, it will be split over multiple APP1 markers
                    this.ExtendProfile(ref this.exifData, profile.AsSpan(Exif00).ToArray());
                }
            }
        }

        /// <summary>
        /// Processes the App2 marker retrieving any stored ICC profile information
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApp2Marker(int remaining)
        {
            // Length is 14 though we only need to check 12.
            const int Icclength = 14;
            if (remaining < Icclength || this.IgnoreMetadata)
            {
                this.InputStream.Skip(remaining);
                return;
            }

            var identifier = new byte[Icclength];
            this.InputStream.Read(identifier, 0, Icclength);
            remaining -= Icclength; // We have read it by this point

            if (ProfileResolver.IsProfile(identifier, ProfileResolver.IccMarker))
            {
                this.isIcc = true;
                var profile = new byte[remaining];
                this.InputStream.Read(profile, 0, remaining);

                if (this.iccData is null)
                {
                    this.iccData = profile;
                }
                else
                {
                    // If the ICC information exceeds 64K, it will be split over multiple APP2 markers
                    this.ExtendProfile(ref this.iccData, profile);
                }
            }
            else
            {
                // Not an ICC profile we can handle. Skip the remaining bytes so we can carry on and ignore this.
                this.InputStream.Skip(remaining);
            }
        }

        /// <summary>
        /// Processes the application header containing the Adobe identifier
        /// which stores image encoding information for DCT filters.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApp14Marker(int remaining)
        {
            const int MarkerLength = AdobeMarker.Length;
            if (remaining < MarkerLength)
            {
                // Skip the application header length
                this.InputStream.Skip(remaining);
                return;
            }

            this.InputStream.Read(this.temp, 0, MarkerLength);
            remaining -= MarkerLength;

            AdobeMarker.TryParse(this.temp, out this.adobe);

            if (remaining > 0)
            {
                this.InputStream.Skip(remaining);
            }
        }

        /// <summary>
        /// Processes the Define Quantization Marker and tables. Specified in section B.2.4.1.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        /// <exception cref="ImageFormatException">
        /// Thrown if the tables do not match the header
        /// </exception>
        private void ProcessDefineQuantizationTablesMarker(int remaining)
        {
            while (remaining > 0)
            {
                bool done = false;
                remaining--;
                int quantizationTableSpec = this.InputStream.ReadByte();
                int tableIndex = quantizationTableSpec & 15;

                // Max index. 4 Tables max.
                if (tableIndex > 3)
                {
                    JpegThrowHelper.ThrowBadQuantizationTable();
                }

                switch (quantizationTableSpec >> 4)
                {
                    case 0:
                    {
                        // 8 bit values
                        if (remaining < 64)
                        {
                            done = true;
                            break;
                        }

                        this.InputStream.Read(this.temp, 0, 64);
                        remaining -= 64;

                        ref Block8x8F table = ref this.QuantizationTables[tableIndex];
                        for (int j = 0; j < 64; j++)
                        {
                            table[j] = this.temp[j];
                        }
                    }

                    break;
                    case 1:
                    {
                        // 16 bit values
                        if (remaining < 128)
                        {
                            done = true;
                            break;
                        }

                        this.InputStream.Read(this.temp, 0, 128);
                        remaining -= 128;

                        ref Block8x8F table = ref this.QuantizationTables[tableIndex];
                        for (int j = 0; j < 64; j++)
                        {
                            table[j] = (this.temp[2 * j] << 8) | this.temp[(2 * j) + 1];
                        }
                    }

                    break;

                    default:
                    {
                        JpegThrowHelper.ThrowBadQuantizationTable();
                        break;
                    }
                }

                if (done)
                {
                    break;
                }
            }

            if (remaining != 0)
            {
                JpegThrowHelper.ThrowBadMarker(nameof(JpegConstants.Markers.DQT), remaining);
            }

            this.Metadata.GetFormatMetadata(JpegFormat.Instance).Quality = QualityEvaluator.EstimateQuality(this.QuantizationTables);
        }

        /// <summary>
        /// Processes the Start of Frame marker.  Specified in section B.2.2.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        /// <param name="frameMarker">The current frame marker.</param>
        /// <param name="metadataOnly">Whether to parse metadata only</param>
        private void ProcessStartOfFrameMarker(int remaining, in JpegFileMarker frameMarker, bool metadataOnly)
        {
            if (this.Frame != null)
            {
                JpegThrowHelper.ThrowImageFormatException("Multiple SOF markers. Only single frame jpegs supported.");
            }

            // Read initial marker definitions.
            const int length = 6;
            this.InputStream.Read(this.temp, 0, length);

            // We only support 8-bit and 12-bit precision.
            if (Array.IndexOf(this.supportedPrecisions, this.temp[0]) == -1)
            {
                JpegThrowHelper.ThrowImageFormatException("Only 8-Bit and 12-Bit precision supported.");
            }

            this.Precision = this.temp[0];

            this.Frame = new JpegFrame
            {
                Extended = frameMarker.Marker == JpegConstants.Markers.SOF1,
                Progressive = frameMarker.Marker == JpegConstants.Markers.SOF2,
                Precision = this.temp[0],
                Scanlines = (short)((this.temp[1] << 8) | this.temp[2]),
                SamplesPerLine = (short)((this.temp[3] << 8) | this.temp[4]),
                ComponentCount = this.temp[5]
            };

            if (this.Frame.SamplesPerLine == 0 || this.Frame.Scanlines == 0)
            {
                JpegThrowHelper.ThrowInvalidImageDimensions(this.Frame.SamplesPerLine, this.Frame.Scanlines);
            }

            this.ImageSizeInPixels = new Size(this.Frame.SamplesPerLine, this.Frame.Scanlines);
            this.ComponentCount = this.Frame.ComponentCount;

            if (!metadataOnly)
            {
                remaining -= length;

                const int componentBytes = 3;
                if (remaining > this.ComponentCount * componentBytes)
                {
                    JpegThrowHelper.ThrowBadMarker("SOFn", remaining);
                }

                this.InputStream.Read(this.temp, 0, remaining);

                // No need to pool this. They max out at 4
                this.Frame.ComponentIds = new byte[this.ComponentCount];
                this.Frame.ComponentOrder = new byte[this.ComponentCount];
                this.Frame.Components = new JpegComponent[this.ComponentCount];
                this.ColorSpace = this.DeduceJpegColorSpace();

                int maxH = 0;
                int maxV = 0;
                int index = 0;
                for (int i = 0; i < this.ComponentCount; i++)
                {
                    byte hv = this.temp[index + 1];
                    int h = (hv >> 4) & 15;
                    int v = hv & 15;

                    if (maxH < h)
                    {
                        maxH = h;
                    }

                    if (maxV < v)
                    {
                        maxV = v;
                    }

                    var component = new JpegComponent(this.configuration.MemoryAllocator, this.Frame, this.temp[index], h, v, this.temp[index + 2], i);

                    this.Frame.Components[i] = component;
                    this.Frame.ComponentIds[i] = component.Id;

                    index += componentBytes;
                }

                this.Frame.MaxHorizontalFactor = maxH;
                this.Frame.MaxVerticalFactor = maxV;
                this.ColorSpace = this.DeduceJpegColorSpace();
                this.Frame.InitComponents();
                this.ImageSizeInMCU = new Size(this.Frame.McusPerLine, this.Frame.McusPerColumn);
            }
        }

        /// <summary>
        /// Processes a Define Huffman Table marker, and initializes a huffman
        /// struct from its contents. Specified in section B.2.4.2.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessDefineHuffmanTablesMarker(int remaining)
        {
            int length = remaining;

            using (IManagedByteBuffer huffmanData = this.configuration.MemoryAllocator.AllocateManagedByteBuffer(256, AllocationOptions.Clean))
            {
                ref byte huffmanDataRef = ref MemoryMarshal.GetReference(huffmanData.GetSpan());
                for (int i = 2; i < remaining;)
                {
                    byte huffmanTableSpec = (byte)this.InputStream.ReadByte();
                    int tableType = huffmanTableSpec >> 4;
                    int tableIndex = huffmanTableSpec & 15;

                    // Types 0..1 DC..AC
                    if (tableType > 1)
                    {
                        JpegThrowHelper.ThrowImageFormatException("Bad Huffman Table type.");
                    }

                    // Max tables of each type
                    if (tableIndex > 3)
                    {
                        JpegThrowHelper.ThrowImageFormatException("Bad Huffman Table index.");
                    }

                    this.InputStream.Read(huffmanData.Array, 0, 16);

                    using (IManagedByteBuffer codeLengths = this.configuration.MemoryAllocator.AllocateManagedByteBuffer(17, AllocationOptions.Clean))
                    {
                        ref byte codeLengthsRef = ref MemoryMarshal.GetReference(codeLengths.GetSpan());
                        int codeLengthSum = 0;

                        for (int j = 1; j < 17; j++)
                        {
                            codeLengthSum += Unsafe.Add(ref codeLengthsRef, j) = Unsafe.Add(ref huffmanDataRef, j - 1);
                        }

                        length -= 17;

                        if (codeLengthSum > 256 || codeLengthSum > length)
                        {
                            JpegThrowHelper.ThrowImageFormatException("Huffman table has excessive length.");
                        }

                        using (IManagedByteBuffer huffmanValues = this.configuration.MemoryAllocator.AllocateManagedByteBuffer(256, AllocationOptions.Clean))
                        {
                            this.InputStream.Read(huffmanValues.Array, 0, codeLengthSum);

                            i += 17 + codeLengthSum;

                            this.BuildHuffmanTable(
                                tableType == 0 ? this.dcHuffmanTables : this.acHuffmanTables,
                                tableIndex,
                                codeLengths.GetSpan(),
                                huffmanValues.GetSpan());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the DRI (Define Restart Interval Marker) Which specifies the interval between RSTn markers, in
        /// macroblocks
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessDefineRestartIntervalMarker(int remaining)
        {
            if (remaining != 2)
            {
                JpegThrowHelper.ThrowBadMarker(nameof(JpegConstants.Markers.DRI), remaining);
            }

            this.resetInterval = this.ReadUint16();
        }

        /// <summary>
        /// Processes the SOS (Start of scan marker).
        /// </summary>
        private void ProcessStartOfScanMarker()
        {
            if (this.Frame is null)
            {
                JpegThrowHelper.ThrowImageFormatException("No readable SOFn (Start Of Frame) marker found.");
            }

            int selectorsCount = this.InputStream.ReadByte();
            for (int i = 0; i < selectorsCount; i++)
            {
                int componentIndex = -1;
                int selector = this.InputStream.ReadByte();

                for (int j = 0; j < this.Frame.ComponentIds.Length; j++)
                {
                    byte id = this.Frame.ComponentIds[j];
                    if (selector == id)
                    {
                        componentIndex = j;
                        break;
                    }
                }

                if (componentIndex < 0)
                {
                    JpegThrowHelper.ThrowImageFormatException($"Unknown component selector {componentIndex}.");
                }

                ref JpegComponent component = ref this.Frame.Components[componentIndex];
                int tableSpec = this.InputStream.ReadByte();
                component.DCHuffmanTableId = tableSpec >> 4;
                component.ACHuffmanTableId = tableSpec & 15;
                this.Frame.ComponentOrder[i] = (byte)componentIndex;
            }

            this.InputStream.Read(this.temp, 0, 3);

            int spectralStart = this.temp[0];
            int spectralEnd = this.temp[1];
            int successiveApproximation = this.temp[2];

            var sd = new HuffmanScanDecoder(
                this.InputStream,
                this.Frame,
                this.dcHuffmanTables,
                this.acHuffmanTables,
                selectorsCount,
                this.resetInterval,
                spectralStart,
                spectralEnd,
                successiveApproximation >> 4,
                successiveApproximation & 15);

            sd.ParseEntropyCodedData();
        }

        /// <summary>
        /// Builds the huffman tables
        /// </summary>
        /// <param name="tables">The tables</param>
        /// <param name="index">The table index</param>
        /// <param name="codeLengths">The codelengths</param>
        /// <param name="values">The values</param>
        [MethodImpl(InliningOptions.ShortMethod)]
        private void BuildHuffmanTable(HuffmanTable[] tables, int index, ReadOnlySpan<byte> codeLengths, ReadOnlySpan<byte> values)
            => tables[index] = new HuffmanTable(codeLengths, values);

        /// <summary>
        /// Reads a <see cref="ushort"/> from the stream advancing it by two bytes
        /// </summary>
        /// <returns>The <see cref="ushort"/></returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        private ushort ReadUint16()
        {
            this.InputStream.Read(this.markerBuffer, 0, 2);
            return BinaryPrimitives.ReadUInt16BigEndian(this.markerBuffer);
        }

        /// <summary>
        /// Post processes the pixels into the destination image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        private Image<TPixel> PostProcessIntoImage<TPixel>()
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (this.ImageWidth == 0 || this.ImageHeight == 0)
            {
                JpegThrowHelper.ThrowInvalidImageDimensions(this.ImageWidth, this.ImageHeight);
            }

            var image = Image.CreateUninitialized<TPixel>(
                this.configuration,
                this.ImageWidth,
                this.ImageHeight,
                this.Metadata);

            using (var postProcessor = new JpegImagePostProcessor(this.configuration, this))
            {
                postProcessor.PostProcess(image.Frames.RootFrame);
            }

            return image;
        }
    }
}
