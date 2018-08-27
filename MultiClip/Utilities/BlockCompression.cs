using System;
using System.IO;
using System.IO.Compression;

namespace MultiClip.Utilities
{
    /// <summary>
    /// Manages the compression (deflation) and 
    /// decompression (inflation) of data.
    /// </summary>
    public static class BlockCompression
    {
        /// <summary>
        /// Deflates the block of data.
        /// </summary>
        public static byte[] Deflate(byte[] buffer)
        {
            int initialCapacity = Math.Min(buffer.Length * 2, 4096);
            using (var memoryStream = new MemoryStream(buffer))
            using (var outputStream = new MemoryStream(initialCapacity))
            using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.Fastest))
            {
                memoryStream.CopyTo(deflateStream);
                deflateStream.Close();
                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Deflates the block of data.
        /// </summary>
        /// <param name="ptr">A pointer to the block of data.</param>
        /// <param name="size">The size of the data in bytes.</param>
        /// <returns>A byte array containing the deflated data</returns>
        public static unsafe byte[] Deflate(byte* ptr, int size)
        {
            int initialCapacity = Math.Min(size * 2, 4096);
            using (var memoryStream = new UnmanagedMemoryStream(ptr, size))
            using (var outputStream = new MemoryStream(initialCapacity))
            using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.Fastest))
            {
                memoryStream.CopyTo(deflateStream);
                deflateStream.Close();
                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Inflates the buffer.
        /// </summary>
        /// <param name="deflatedBuffer">The deflated buffer.</param>
        /// <param name="inflatedSize">The size of the buffer when inflated.</param>
        /// <param name="destination">The target buffer.</param>
        public static void Inflate(byte[] deflatedBuffer, int inflatedSize, byte[] destination)
        {
            using (var sourceStream = new MemoryStream(deflatedBuffer))
            using (var decompressor = new DeflateStream(sourceStream, CompressionMode.Decompress))
            {
                decompressor.Read(destination, 0, inflatedSize);
                decompressor.Close();
            }
        }

        /// <summary>
        /// Inflates the buffer.
        /// </summary>
        /// <param name="deflatedBuffer">The deflated buffer.</param>
        /// <param name="inflatedSize">The size of the buffer when inflated.</param>
        /// <param name="destination">A pointer to the destination.</param>
        public static unsafe void Inflate(byte[] deflatedBuffer, int inflatedSize, byte* destination)
        {
            using (var memoryStream = new UnmanagedMemoryStream(destination, inflatedSize, inflatedSize, FileAccess.ReadWrite))
            using (var sourceStream = new MemoryStream(deflatedBuffer))
            using (var inflateStream = new DeflateStream(sourceStream, CompressionMode.Decompress))
            {
                inflateStream.CopyTo(memoryStream);
                inflateStream.Close();
            }
        }
    }
}
