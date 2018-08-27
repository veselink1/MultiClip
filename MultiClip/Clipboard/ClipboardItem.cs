using System;
using System.Collections.Generic;
using MultiClip.Native;
using MultiClip.Utilities;

namespace MultiClip.Clipboard
{
    public sealed class ClipboardItem
    {
        /// <summary>
        /// The global ID of the format.
        /// </summary>
        public DataFormat Format { get; private set; } 

        /// <summary>
        /// The string representation of the data format.
        /// </summary>
        public string FormatName => System.Windows.DataFormats.GetDataFormat((int)Format).Name;

        /// <summary>
        /// The size of the buffer.
        /// </summary>
        public int Size => _actualSize;

        /// <summary>
        /// The size of the clipboard item currently in memory.
        /// </summary>
        public int SizeInMemory => _buffer.Length;

        private bool _isDeflated;
        // The compressed byte buffer representing the data.
        private byte[] _buffer;
        // A weak reference to the decomressed buffer. 
        // Valid only when isDeflated is true.
        private WeakReference<byte[]> _weakInflatedBufferRef = new WeakReference<byte[]>(null);
        // The size of the decompressed buffer.
        private int _actualSize;

        public static ClipboardItem FromBuffer(DataFormat formatId, byte[] buffer, bool cloneBuffer = true, bool optimizeLongTerm = true)
        {
            if (optimizeLongTerm && buffer.Length >= 5 * 1024 * 1024) // 5MiBVV  
            {
                byte[] deflatedBuffer = BlockCompression.Deflate(buffer);
                float deflationRatio = (float)buffer.Length / deflatedBuffer.Length;
                if (deflationRatio > 1.5f)
                {
                    return new ClipboardItem(formatId, deflatedBuffer, buffer.Length, isDeflated: true);
                }
            }

            return new ClipboardItem(formatId, cloneBuffer ? (byte[])buffer.Clone() : buffer, buffer.Length, isDeflated: false);
        }

        public static unsafe ClipboardItem FromPointer(DataFormat formatId, byte* bufferPtr, int size)
        {
            if (size > 4096)
            {
                byte[] deflatedBuffer = BlockCompression.Deflate(bufferPtr, size);
                float deflationRatio = (float)size/ deflatedBuffer.Length;
                if (deflationRatio > 1.5f)
                {
                    return new ClipboardItem(formatId, deflatedBuffer, size, isDeflated: true);
                }
            }

            byte[] newBuffer = new byte[size];
            fixed (byte* dest = newBuffer)
            {
                Stdlib.MemCpy(dest, bufferPtr, (UIntPtr)size);
            }
            return new ClipboardItem(formatId, newBuffer, newBuffer.Length, isDeflated: false);
        }

        private ClipboardItem(DataFormat format, byte[] buffer, int finalSize, bool isDeflated)
        {
            Format = format;
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _actualSize = finalSize;
            _isDeflated = isDeflated;
        }
        
        /// <summary>
        /// Loads the data as a byte array.
        /// </summary>
        /// <returns>The byte array representation of the data.</returns>
        public byte[] GetDataBuffer()
        {
            if (_buffer == null)
            {
                throw new InvalidOperationException();
            }

            if (_isDeflated)
            {
                if (!_weakInflatedBufferRef.TryGetTarget(out byte[] inflatedBuffer))
                {
                    inflatedBuffer = new byte[_actualSize];
                    BlockCompression.Inflate(_buffer, _actualSize, inflatedBuffer);
                    _weakInflatedBufferRef.SetTarget(inflatedBuffer);
                }
                return inflatedBuffer;
            }
            else
            {
                return _buffer;
            }
        }

        /// <summary>
        /// Loads the buffer into the pointed location.
        /// </summary>
        /// <param name="pDestination">A pointer to the destination of the data.</param>
        public unsafe void CopyTo(byte* pDestination)
        {
            if (_isDeflated)
            {
                if (_weakInflatedBufferRef.TryGetTarget(out var buffer))
                {
                    fixed (byte* pBuffer = buffer)
                    {
                        Stdlib.MemCpy(pDestination, pBuffer, (UIntPtr)_actualSize);
                    }
                }
                else
                {
                    // Decompress the buffer of the ClipboardItem into the destination.
                    BlockCompression.Inflate(_buffer, _actualSize, pDestination);
                }
            }
            else
            {
                fixed (byte* pBuffer = _buffer)
                {
                    Stdlib.MemCpy(pDestination, pBuffer, (UIntPtr)_actualSize);
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ClipboardItem other)
            {
                if (Format != other.Format)
                    return false;

                if (_isDeflated && other._isDeflated)
                {
                    return Stdlib.MemCmp(_buffer, other._buffer);
                }
                else
                {
                    return Stdlib.MemCmp(GetDataBuffer(), other.GetDataBuffer());
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var hashCode = 1133383785;
            hashCode = hashCode * -1521134295 + Format.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(_buffer);
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }
    }
}
