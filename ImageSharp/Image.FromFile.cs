// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp
{
    /// <content>
    /// Adds static methods allowing the creation of new image from a given file.
    /// </content>
    public abstract partial class Image
    {
        /// <summary>
        /// By reading the header on the provided file this calculates the images mime type.
        /// </summary>
        /// <param name="filePath">The image file to open and to read the header from.</param>
        /// <returns>The mime type or null if none found.</returns>
        public static IImageFormat DetectFormat(string filePath)
        {
            return DetectFormat(Configuration.Default, filePath);
        }

        /// <summary>
        /// By reading the header on the provided file this calculates the images mime type.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="filePath">The image file to open and to read the header from.</param>
        /// <returns>The mime type or null if none found.</returns>
        public static IImageFormat DetectFormat(Configuration config, string filePath)
        {
            config = config ?? Configuration.Default;
            using (Stream file = config.FileSystem.OpenRead(filePath))
            {
                return DetectFormat(config, file);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(string path) => Load(Configuration.Default, path);

        /// <summary>
        /// Create a new instance of the <see cref="Image"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <param name="format">The mime type of the decoded image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>A new <see cref="Image{Rgba32}"/>.</returns>
        public static Image Load(string path, out IImageFormat format) => Load(Configuration.Default, path, out format);

        /// <summary>
        /// Create a new instance of the <see cref="Image"/> class from the given file.
        /// </summary>
        /// <param name="config">The config for the decoder.</param>
        /// <param name="path">The file path to the image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(Configuration config, string path) => Load(config, path, out _);

        /// <summary>
        /// Create a new instance of the <see cref="Image"/> class from the given file.
        /// </summary>
        /// <param name="config">The Configuration.</param>
        /// <param name="path">The file path to the image.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(Configuration config, string path, IImageDecoder decoder)
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load(config, stream, decoder);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Image"/>.</returns>
        public static Image Load(string path, IImageDecoder decoder) => Load(Configuration.Default, path, decoder);

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image<TPixel> Load<TPixel>(string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return Load<TPixel>(Configuration.Default, path);
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <param name="format">The mime type of the decoded image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image<TPixel> Load<TPixel>(string path, out IImageFormat format)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return Load<TPixel>(Configuration.Default, path, out format);
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given file.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="path">The file path to the image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image<TPixel> Load<TPixel>(Configuration config, string path)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load<TPixel>(config, stream);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given file.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="path">The file path to the image.</param>
        /// <param name="format">The mime type of the decoded image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image<TPixel> Load<TPixel>(Configuration config, string path, out IImageFormat format)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load<TPixel>(config, stream, out format);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image"/> class from the given file.
        /// The pixel type is selected by the decoder.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="path">The file path to the image.</param>
        /// <param name="format">The mime type of the decoded image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image Load(Configuration config, string path, out IImageFormat format)
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load(config, stream, out format);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image<TPixel> Load<TPixel>(string path, IImageDecoder decoder)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return Load<TPixel>(Configuration.Default, path, decoder);
        }

        /// <summary>
        /// Create a new instance of the <see cref="Image{TPixel}"/> class from the given file.
        /// </summary>
        /// <param name="config">The Configuration.</param>
        /// <param name="path">The file path to the image.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>A new <see cref="Image{TPixel}"/>.</returns>
        public static Image<TPixel> Load<TPixel>(Configuration config, string path, IImageDecoder decoder)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load<TPixel>(config, stream, decoder);
            }
        }
    }
}
