// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace SixLabors.ImageSharp.Processing.Processors.Quantization
{
    /// <summary>
    /// Contains utility methods for <see cref="IFrameQuantizer{TPixel}"/> instances.
    /// </summary>
    public static class FrameQuantizerUtilities
    {
        /// <summary>
        /// Helper method for throwing an exception when a frame quantizer palette has
        /// been requested but not built yet.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="palette">The frame quantizer palette.</param>
        /// <exception cref="InvalidOperationException">
        /// The palette has not been built via <see cref="IFrameQuantizer{TPixel}.BuildPalette(ImageFrame{TPixel}, Rectangle)"/>
        /// </exception>
        public static void CheckPaletteState<TPixel>(in ReadOnlyMemory<TPixel> palette)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (palette.Equals(default))
            {
                throw new InvalidOperationException("Frame Quantizer palette has not been built.");
            }
        }

        /// <summary>
        /// Quantizes an image frame and return the resulting output pixels.
        /// </summary>
        /// <typeparam name="TFrameQuantizer">The type of frame quantizer.</typeparam>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="quantizer">The frame quantizer.</param>
        /// <param name="source">The source image frame to quantize.</param>
        /// <param name="bounds">The bounds within the frame to quantize.</param>
        /// <returns>
        /// A <see cref="IndexedImageFrame{TPixel}"/> representing a quantized version of the source frame pixels.
        /// </returns>
        public static IndexedImageFrame<TPixel> QuantizeFrame<TFrameQuantizer, TPixel>(
            ref TFrameQuantizer quantizer,
            ImageFrame<TPixel> source,
            Rectangle bounds)
            where TFrameQuantizer : struct, IFrameQuantizer<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Guard.NotNull(source, nameof(source));
            var interest = Rectangle.Intersect(source.Bounds(), bounds);

            // Collect the palette. Required before the second pass runs.
            quantizer.BuildPalette(source, interest);

            var destination = new IndexedImageFrame<TPixel>(
                quantizer.Configuration,
                interest.Width,
                interest.Height,
                quantizer.Palette);

            if (quantizer.Options.Dither is null)
            {
                SecondPass(ref quantizer, source, destination, interest);
            }
            else
            {
                // We clone the image as we don't want to alter the original via error diffusion based dithering.
                using ImageFrame<TPixel> clone = source.Clone();
                SecondPass(ref quantizer, clone, destination, interest);
            }

            return destination;
        }

        [MethodImpl(InliningOptions.ShortMethod)]
        private static void SecondPass<TFrameQuantizer, TPixel>(
            ref TFrameQuantizer quantizer,
            ImageFrame<TPixel> source,
            IndexedImageFrame<TPixel> destination,
            Rectangle bounds)
            where TFrameQuantizer : struct, IFrameQuantizer<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            IDither dither = quantizer.Options.Dither;

            if (dither is null)
            {
                var operation = new RowIntervalOperation<TFrameQuantizer, TPixel>(
                    ref quantizer,
                    source,
                    destination,
                    bounds);

                ParallelRowIterator.IterateRowIntervals(
                    quantizer.Configuration,
                    bounds,
                    in operation);

                return;
            }

            dither.ApplyQuantizationDither(ref quantizer, source, destination, bounds);
        }

        private readonly struct RowIntervalOperation<TFrameQuantizer, TPixel> : IRowIntervalOperation
            where TFrameQuantizer : struct, IFrameQuantizer<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly TFrameQuantizer quantizer;
            private readonly ImageFrame<TPixel> source;
            private readonly IndexedImageFrame<TPixel> destination;
            private readonly Rectangle bounds;

            [MethodImpl(InliningOptions.ShortMethod)]
            public RowIntervalOperation(
                ref TFrameQuantizer quantizer,
                ImageFrame<TPixel> source,
                IndexedImageFrame<TPixel> destination,
                Rectangle bounds)
            {
                this.quantizer = quantizer;
                this.source = source;
                this.destination = destination;
                this.bounds = bounds;
            }

            [MethodImpl(InliningOptions.ShortMethod)]
            public void Invoke(in RowInterval rows)
            {
                int offsetY = this.bounds.Top;
                int offsetX = this.bounds.Left;

                for (int y = rows.Min; y < rows.Max; y++)
                {
                    Span<TPixel> sourceRow = this.source.GetPixelRowSpan(y);
                    Span<byte> destinationRow = this.destination.GetWritablePixelRowSpanUnsafe(y - offsetY);

                    for (int x = this.bounds.Left; x < this.bounds.Right; x++)
                    {
                        destinationRow[x - offsetX] = Unsafe.AsRef(this.quantizer).GetQuantizedColor(sourceRow[x], out TPixel _);
                    }
                }
            }
        }
    }
}
