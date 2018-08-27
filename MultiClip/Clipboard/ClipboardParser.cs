using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using MultiClip.Native;
using MultiClip.Utilities;

namespace MultiClip.Clipboard
{
    /// <summary>
    /// Allows the user to parse data from a clipboard fragment.
    /// </summary>
    public static class ClipboardParser
    {
        /// <summary>
        /// A list of all the clipboard formats that will be treated as text.
        /// <see cref="ParseText(ClipboardItem)"/>
        /// </summary>
        private static readonly DataFormat[] TextDataTypesIDs = 
        {
            DataFormat.Text,
            // DataFormat.OemText, (implicitly convertible to UnicodeText)
            DataFormat.UnicodeText,
        };

        /// <summary>
        /// A list of all the clipboard formats that will be treated as images. 
        /// <see cref="ParseImage(ClipboardItem)"/>
        /// </summary>
        private static readonly DataFormat[] ImageDataTypesIDs =
        {
            // DataFormat.Bitmap, (implicitly convertible to Dib)
            DataFormat.Dib,
            // DataFormat.DibV5, (implicitly convertible to Dib)
            // DataFormat.Tiff, (implicitly convertible to Dib)
        };

        /// <summary>
        /// A list of all the clipboard formats that will be treated as file drops.
        /// <see cref="ParseFileDrop(ClipboardItem)"/>
        /// </summary>
        private static readonly DataFormat[] FileDropTypesIDs =
        {
            DataFormat.HDrop,
            // DataFormat.ShellIdList, (very difficult to support and rarely needed)
        };

        private static readonly DataFormat[] FormatsByPreference = new DataFormat[]
        {
            DataFormat.Dib,
            DataFormat.HDrop,
            DataFormat.UnicodeText,
            DataFormat.Text,
        };

        public static AbstractDataFormat GetAbstractFormat(DataFormat format)
        {
            if (ImageDataTypesIDs.Contains(format))
                return AbstractDataFormat.Image;
            else if (FileDropTypesIDs.Contains(format))
                return AbstractDataFormat.FileDrop;
            else if (TextDataTypesIDs.Contains(format))
                return AbstractDataFormat.Text;
            else
                return AbstractDataFormat.Unknown;
        }

        /// <summary>
        /// Checks if the ClipboardItem contains data 
        /// that can be parsed as the requested type.
        /// </summary>
        /// <param name="item">The ClipboardItem.</param>
        /// <param name="type">The requested abstract data type.</param>
        /// <returns>True when the fragment contains data that can be parsed as the requested type.</returns>
        public static bool CanParse(ClipboardItem item, AbstractDataFormat type)
        {
            return GetAbstractFormat(item.Format) == type;
        }

        public static bool CanSerialize(AbstractDataFormat type)
        {
            return type == AbstractDataFormat.Image || type == AbstractDataFormat.Text;
        }

        public static ClipboardItem GetPreferredItem(IEnumerable<ClipboardItem> items, bool serializable)
        {
            if (serializable)
            {
                items = items.Where(x => CanSerialize(GetAbstractFormat(x.Format)));
            }

            return items
                .OrderByDescending(x => GetFormatPreference(x.Format))
                .FirstOrDefault();
        }

        public static byte[] GetBytes(ClipboardItem item, AbstractDataFormat type)
        {
            switch (type)
            {
                case AbstractDataFormat.Image:
                {
                    BitmapFrame frame = ParseImage(item);
                    PngBitmapEncoder png = new PngBitmapEncoder();
                    png.Frames.Add(frame);
                    using (MemoryStream mem = new MemoryStream())
                    {
                        png.Save(mem);
                        return mem.ToArray();
                    }
                }
                case AbstractDataFormat.Text:
                {
                    string text = ParseText(item);
                    return Encoding.Unicode.GetBytes(text);
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool WeakEquals(ClipboardState stateA, ClipboardState stateB)
        {
            if (stateA.Items.Count == 0)
            {
                return stateB.Items.Count == 0;
            }

            var itemA = GetPreferredItem(stateA.Items, serializable: true);
            if (itemA != null)
            {
                var itemB = stateB.Items.FirstOrDefault(x => x.Format == itemA.Format);
                return itemA.Equals(itemB);
            }
            else
            {
                if (stateA.Items.Count != stateB.Items.Count
                    || stateA.Items.Any(x => !stateB.Items.Any(y => x.Equals(y))))
                {
                    return false;
                }
                return true;
            }
        }

        public static BitmapFrame ImageFromBytes(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return decoder.Frames[0];
            }
        }

        public static string TextFromBytes(byte[] buffer)
        {
            return Encoding.Unicode.GetString(buffer);
        }

        /// <summary>
        /// Parses the ClipboardItem as text (string).
        /// </summary>
        public static string ParseText(ClipboardItem item)
        {
            if (!CanParse(item, AbstractDataFormat.Text))
            {
                throw new ArgumentException("The ClipboardItem does not contain data in the requested format.");
            }

            switch (item.Format)
            {
                case DataFormat.Text:
                    return ParseStringAZ(item.GetDataBuffer());
                case DataFormat.UnicodeText:
                    return ParseStringWZ(item.GetDataBuffer());
                case DataFormat.OemText:
                    return ParseStringAZ(item.GetDataBuffer());
                default:
                    throw new NotImplementedException();
            }
        }

        private static int GetFormatPreference(DataFormat type)
        {
            int index = Array.IndexOf(FormatsByPreference, type);
            if (index == -1)
            {
                return 0;
            }

            return FormatsByPreference.Length - index;
        }

        /// <summary>
        /// Parses a byte buffer as a (possibly zero-terminated) ASCII string.
        /// </summary>
        private static string ParseStringAZ(byte[] bytes)
        {
            int length = 0;
            for (; length < bytes.Length && bytes[length] != 0; length++)
                continue;
            return Encoding.ASCII.GetString(bytes, 0, length);
        }

        /// <summary>
        /// Parses a byte buffer as a (possible zero-terminated) unicode string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string ParseStringWZ(byte[] bytes)
        {
            int length = 0;
            for (; length < bytes.Length - 1 && (bytes[length] != 0 || bytes[length + 1] != 0); length += 2)
                continue;
            return Encoding.Unicode.GetString(bytes, 0, length);
        }

        /// <summary>
        /// Parses the ClipboardItem as an image (BitmapFrame).
        /// </summary>
        public static BitmapFrame ParseImage(ClipboardItem item)
        {
            if (!CanParse(item, AbstractDataFormat.Image))
            {
                throw new ArgumentException("The ClipboardItem does not contain data in the requested format.");
            }

            switch (item.Format)
            {
                case DataFormat.Dib:
                    byte[] dibBytes = item.GetDataBuffer();
                    BitmapFrame imageSource = DIBitmap.CreateBitmapFrameFromDibBytes(dibBytes);
                    // Freeze the image to prevent modifications
                    // and allow it to be used from different threads.
                    imageSource.Freeze();
                    return imageSource;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Parses the ClipboardItem as a FileDrop (string[]).
        /// </summary>
        public static string[] ParseFileDrop(ClipboardItem item)
        {
            if (!CanParse(item, AbstractDataFormat.FileDrop))
            {
                throw new ArgumentException("The ClipboardItem does not contain data in the requested format.");
            }

            switch (item.Format)
            {
                case DataFormat.HDrop:
                {
                    byte[] bytes = item.GetDataBuffer();
                    // The first 18 bytes are object header and size information.
                    // The rest is a block of zero-terminated unicode strings.
                    string fileNamesZero = Encoding.Unicode.GetString(bytes, 18, bytes.Length - 18);
                    string[] fileNames = fileNamesZero
                        .Split('\0')
                        .Where(x => x.Length > 0)
                        .ToArray();

                    return fileNames;
                }
                case DataFormat.ShellIdList:
                {
                    byte[] bytes = item.GetDataBuffer();
                    NativeMethods.CIDA cida = BinaryStructConverter.FromByteArray<NativeMethods.CIDA>(bytes);

                    return new string[] { };
                }
                default:
                    throw new NotImplementedException();
            }
        }
        
        private static int IndexOf(byte[] buffer, byte[] pattern)
        {
            int n = buffer.Length - pattern.Length + 1;
            for (int i = 0; i < n; i++)
            {
                if (buffer[i] == pattern[0])
                {
                    bool isMatch = true;
                    for (int j = 1; j < pattern.Length; j++)
                    {
                        if (buffer[i + j] != pattern[j])
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
