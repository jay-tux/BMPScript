// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Filters;

namespace SixLabors.ImageSharp.Processing.Processors.Convolution
{
    /// <summary>
    /// Defines a processor that detects edges within an image using two one-dimensional matrices.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal class EdgeDetector2DProcessor<TPixel> : ImageProcessor<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeDetector2DProcessor{TPixel}"/> class.
        /// </summary>
        /// <param name="configuration">The configuration which allows altering default behaviour or extending the library.</param>
        /// <param name="kernelX">The horizontal gradient operator.</param>
        /// <param name="kernelY">The vertical gradient operator.</param>
        /// <param name="grayscale">Whether to convert the image to grayscale before performing edge detection.</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance.</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance.</param>
        internal EdgeDetector2DProcessor(
            Configuration configuration,
            in DenseMatrix<float> kernelX,
            in DenseMatrix<float> kernelY,
            bool grayscale,
            Image<TPixel> source,
            Rectangle sourceRectangle)
            : base(configuration, source, sourceRectangle)
        {
            Guard.IsTrue(kernelX.Size.Equals(kernelY.Size), $"{nameof(kernelX)} {nameof(kernelY)}", "Kernel sizes must be the same.");
            this.KernelX = kernelX;
            this.KernelY = kernelY;
            this.Grayscale = grayscale;
        }

        /// <summary>
        /// Gets the horizontal gradient operator.
        /// </summary>
        public DenseMatrix<float> KernelX { get; }

        /// <summary>
        /// Gets the vertical gradient operator.
        /// </summary>
        public DenseMatrix<float> KernelY { get; }

        public bool Grayscale { get; }

        /// <inheritdoc/>
        protected override void BeforeImageApply()
        {
            if (this.Grayscale)
            {
                new GrayscaleBt709Processor(1F).Execute(this.Configuration, this.Source, this.SourceRectangle);
            }

            base.BeforeImageApply();
        }

        /// <inheritdoc />
        protected override void OnFrameApply(ImageFrame<TPixel> source)
        {
            using var processor = new Convolution2DProcessor<TPixel>(this.Configuration, this.KernelX, this.KernelY, true, this.Source, this.SourceRectangle);

            processor.Apply(source);
        }
    }
}
