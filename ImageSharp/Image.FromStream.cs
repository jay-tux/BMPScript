// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp
{
    /// <content>
    /// Adds static methods allowing the creation of new image from a given stream.
    /// </content>
    public abstract partial class Image
    {
        /// <summary>
        /// By reading the header on the provided stream this calculates the images format type.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>The format type or null if none found.</returns>
        public static IImageFormat DetectFormat(Stream stream) => DetectFormat(Configuration.Default, stream);

        /// <summary>
        /// By reading the header on the provided stream this calculates the images format type.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>The format type or null if none found.</returns>
        public static IImageFormat DetectFormat(Configuration config, Stream stream)
            => WithSeekableStream(config, stream, s => InternalDetectFormat(s, config));

        /// <summary>
        /// By reading the header on the provided stream this reads the raw image information.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>
        /// The <see cref="IImageInfo"/> or null if suitable info detector not found.
        /// </returns>
        public static IImageInfo Identify(Stream stream) => Identify(stream, out IImageFormat _);

        /// <summary>
        /// By reading the header on the provided stream this reads the raw image information.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>
        /// The <see cref="IImageInfo"/> or null if suitable info detector not found.
        /// </returns>
        public static IImageInfo Identify(Stream stream, out IImageFormat format) => Identify(Configuration.Default, stream, out format);

        /// <summary>
        /// Reads the raw image information from the specified stream without fully decoding it.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="stream">The image stream to read the information from.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>
        /// The <see cref="IImageInfo"/> or null if suitable info detector is not found.
        /// </returns>
        public static IImageInfo Identify(Configuration config, Stream stream, out IImageFormat format)
        {
            (IImageInfo info, IImageFormat format) data = WithSeekableStream(config, stream, s => InternalIdentity(s, config ?? Configuration.Default));

            format = data.format;
            return data.info;
        }

        /// <summary>
        /// Decode a new instance of the <see cref="Image"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(Stream stream, out IImageFormat format) => Load(Configuration.Default, stream, out format);

        /// <summary>
        /// Decode a new instance of the <see cref="Image"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(Stream stream) => Load(Configuration.Default, stream);

        /// <summary>
        /// Decode a new instance of the <see cref="Image"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(Stream stream, IImageDecoder decoder) => Load(Configuration.Default, stream, decoder);

        /// <summary>
        /// Decode a new instance of the <see cref="Image"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="config">The config for the decoder.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <returns>A new <see cref="Image"/>.</returns>>
        public static Image Load(Configuration config, Stream stream, IImageDecoder decoder) =>
            WithSeekableStream(config, stream, s => decoder.Decode(config, s));

        /// <summary>
        /// Decode a new instance of the <see cref="Image"/> class from the given stream.
        /// </summary>
        /// <param name="config">The config for the decoder.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <returns>A new <see cref="Image"/>.</returns>>
        public static Image Load(Configuration config, Stream stream) => Load(config, stream, out _);

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given stream.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>>
        public static Image<TPixel> Load<TPixel>(Stream stream)
            where TPixel : unmanaged, IPixel<TPixel>
            => Load<TPixel>(null, stream);

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given stream.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>>
        public static Image<TPixel> Load<TPixel>(Stream stream, out IImageFormat format)
            where TPixel : unmanaged, IPixel<TPixel>
            => Load<TPixel>(null, stream, out format);

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given stream.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>>
        public static Image<TPixel> Load<TPixel>(Stream stream, IImageDecoder decoder)
            where TPixel : unmanaged, IPixel<TPixel>
            => WithSeekableStream(Configuration.Default, stream, s => decoder.Decode<TPixel>(Configuration.Default, s));

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given stream.
        /// </summary>
        /// <param name="config">The Configuration.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>>
        public static Image<TPixel> Load<TPixel>(Configuration config, Stream stream, IImageDecoder decoder)
            where TPixel : unmanaged, IPixel<TPixel>
            => WithSeekableStream(config, stream, s => decoder.Decode<TPixel>(config, s));

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given stream.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>>
        public static Image<TPixel> Load<TPixel>(Configuration config, Stream stream)
            where TPixel : unmanaged, IPixel<TPixel>
            => Load<TPixel>(config, stream, out IImageFormat _);

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given stream.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>>
        public static Image<TPixel> Load<TPixel>(Configuration config, Stream stream, out IImageFormat format)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            config = config ?? Configuration.Default;
            (Image<TPixel> img, IImageFormat format) data = WithSeekableStream(config, stream, s => Decode<TPixel>(s, config));

            format = data.format;

            if (data.img != null)
            {
                return data.img;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Image cannot be loaded. Available decoders:");

            foreach (KeyValuePair<IImageFormat, IImageDecoder> val in config.ImageFormatsManager.ImageDecoders)
            {
                sb.AppendLine($" - {val.Key.Name} : {val.Value.GetType().Name}");
            }

            throw new UnknownImageFormatException(sb.ToString());
        }

        /// <summary>
        /// Decode a new instance of the <see cref="Image"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownImageFormatException">Image cannot be loaded.</exception>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image Load(Configuration config, Stream stream, out IImageFormat format)
        {
            config = config ?? Configuration.Default;
            (Image img, IImageFormat format) data = WithSeekableStream(config, stream, s => Decode(s, config));

            format = data.format;

            if (data.img != null)
            {
                return data.img;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Image cannot be loaded. Available decoders:");

            foreach (KeyValuePair<IImageFormat, IImageDecoder> val in config.ImageFormatsManager.ImageDecoders)
            {
                sb.AppendLine($" - {val.Key.Name} : {val.Value.GetType().Name}");
            }

            throw new UnknownImageFormatException(sb.ToString());
        }

        private static T WithSeekableStream<T>(Configuration config, Stream stream, Func<Stream, T> action)
        {
            if (!stream.CanRead)
            {
                throw new NotSupportedException("Cannot read from the stream.");
            }

            if (stream.CanSeek)
            {
                if (config.ReadOrigin == ReadOrigin.Begin)
                {
                    stream.Position = 0;
                }

                return action(stream);
            }

            // We want to be able to load images from things like HttpContext.Request.Body
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                return action(memoryStream);
            }
        }
    }
}
