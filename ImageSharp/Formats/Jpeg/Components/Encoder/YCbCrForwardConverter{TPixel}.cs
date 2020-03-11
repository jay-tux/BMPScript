// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Formats.Jpeg.Components.Encoder
{
    /// <summary>
    /// On-stack worker struct to efficiently encapsulate the TPixel -> Rgb24 -> YCbCr conversion chain of 8x8 pixel blocks.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type to work on</typeparam>
    internal ref struct YCbCrForwardConverter<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// The Y component
        /// </summary>
        public Block8x8F Y;

        /// <summary>
        /// The Cb component
        /// </summary>
        public Block8x8F Cb;

        /// <summary>
        /// The Cr component
        /// </summary>
        public Block8x8F Cr;

        /// <summary>
        /// The color conversion tables
        /// </summary>
        private RgbToYCbCrTables colorTables;

        /// <summary>
        /// Temporal 8x8 block to hold TPixel data
        /// </summary>
        private GenericBlock8x8<TPixel> pixelBlock;

        /// <summary>
        /// Temporal RGB block
        /// </summary>
        private GenericBlock8x8<Rgb24> rgbBlock;

        public static YCbCrForwardConverter<TPixel> Create()
        {
            var result = default(YCbCrForwardConverter<TPixel>);
            result.colorTables = RgbToYCbCrTables.Create();
            return result;
        }

        /// <summary>
        /// Converts a 8x8 image area inside 'pixels' at position (x,y) placing the result members of the structure (<see cref="Y"/>, <see cref="Cb"/>, <see cref="Cr"/>)
        /// </summary>
        public void Convert(ImageFrame<TPixel> frame, int x, int y, in RowOctet<TPixel> currentRows)
        {
            this.pixelBlock.LoadAndStretchEdges(frame.PixelBuffer, x, y, currentRows);

            Span<Rgb24> rgbSpan = this.rgbBlock.AsSpanUnsafe();
            PixelOperations<TPixel>.Instance.ToRgb24(frame.GetConfiguration(), this.pixelBlock.AsSpanUnsafe(), rgbSpan);

            ref float yBlockStart = ref Unsafe.As<Block8x8F, float>(ref this.Y);
            ref float cbBlockStart = ref Unsafe.As<Block8x8F, float>(ref this.Cb);
            ref float crBlockStart = ref Unsafe.As<Block8x8F, float>(ref this.Cr);
            ref Rgb24 rgbStart = ref rgbSpan[0];

            for (int i = 0; i < 64; i++)
            {
                ref Rgb24 c = ref Unsafe.Add(ref rgbStart, i);

                this.colorTables.ConvertPixelInto(
                    c.R,
                    c.G,
                    c.B,
                    ref Unsafe.Add(ref yBlockStart, i),
                    ref Unsafe.Add(ref cbBlockStart, i),
                    ref Unsafe.Add(ref crBlockStart, i));
            }
        }
    }
}
