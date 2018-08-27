using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MultiClip.Clipboard
{
    public enum DataFormat : uint
    {
        /// <summary>
        /// Format ID specifying an ASCII string.
        /// </summary>
        Text = 1,
        /// <summary>
        /// Format ID specifying a bitmap image.
        /// </summary>
        Bitmap = 2,
        /// <summary>
        /// Format ID specifying a metafile picture.
        /// </summary>
        MetaFilePict = 3,
        /// <summary>
        /// Format ID specifying a symbolic link.
        /// </summary>
        Sylk = 4,
        /// <summary>
        /// Format ID specifying data in the Data Interchange Format (DIF).
        /// </summary>
        Dif = 5,
        /// <summary>
        /// Format ID specifying a Tagged Image File Format (TIFF).
        /// </summary>
        Tiff = 6,
        /// <summary>
        /// Format ID specifying a standard Windows original equipment manufacturer (OEM) text.
        /// </summary>
        OemText = 7,
        /// <summary>
        /// Format ID specifying a DIB.
        /// </summary>
        Dib = 8,
        /// <summary>
        /// Format ID specifying a palette.
        /// </summary>
        Palette = 9,
        /// <summary>
        /// Format ID specifying pen data.
        /// </summary>
        PenData = 10,
        /// <summary>
        /// Format ID specifying Resource Interchange File Format (RIFF) audio.
        /// </summary>
        Riff = 11,
        /// <summary>
        /// Format ID specifying wave audio.
        /// </summary>
        Wave = 12,
        /// <summary>
        /// Format ID specifying an Unicode string.
        /// </summary>
        UnicodeText = 13,
        /// <summary>
        /// Format ID specifying an enhanced metafile.
        /// </summary>
        EnhMetaFile = 14,
        /// <summary>
        /// Format ID specifying a file drop.
        /// </summary>
        HDrop = 15,
        /// <summary>
        /// Format ID specifying a locale.
        /// </summary>
        Locale = 16,
        /// <summary>
        /// Format ID specifying a DIB v5.
        /// </summary>
        DibV5 = 17,
        /// <summary>
        /// Format ID specifying a CIDA structure.
        /// </summary>
        ShellIdList = 49382,
    }

    /// <summary>
    /// An enumeration listing the different 
    /// abstract clipboard data types.
    /// </summary>
    public enum AbstractDataFormat
    {
        Unknown = -1,
        /// <summary>
        /// Represents any kind of textual data. 
        /// </summary>
        Text,
        /// <summary>
        /// Represents any kind of imagery.
        /// </summary>
        Image,
        /// <summary>
        /// Represents any kind of file drop list.
        /// </summary>
        FileDrop
    }
    
    /// <summary>
    /// Represents an automatic conversion.
    /// </summary>
    public struct DataConversion : IEquatable<DataConversion>
    {
        public DataFormat Source { get; private set; }
        public DataFormat Target { get; private set; }

        public DataConversion(DataFormat source, DataFormat target)
        {
            Source = source;
            Target = target;
        }

        public bool Equals(DataConversion other)
        {
            return Source == other.Source && Target == other.Target;
        }

        public override bool Equals(object obj)
        {
            return obj is DataConversion && Equals((DataConversion)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -1031959520;
            hashCode = hashCode * -1521134295 + Source.GetHashCode();
            hashCode = hashCode * -1521134295 + Target.GetHashCode();
            return hashCode;
        }

        public static bool operator==(DataConversion a, DataConversion b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(DataConversion a, DataConversion b)
        {
            return !a.Equals(b);
        }
    }

    public static class DataConversions
    {

        /// <summary>
        /// A list of the default synthesized (automatic) conversions that the 
        /// Windows OS can do by default. 
        /// </summary>
        private static readonly IReadOnlyList<DataConversion> s_defaultConversions = new ReadOnlyCollection<DataConversion>(new DataConversion[]
        {
            new DataConversion(DataFormat.Bitmap, DataFormat.Dib),
            new DataConversion(DataFormat.Bitmap, DataFormat.DibV5),
            new DataConversion(DataFormat.Dib, DataFormat.Bitmap),
            new DataConversion(DataFormat.Dib, DataFormat.Palette),
            new DataConversion(DataFormat.Dib, DataFormat.DibV5),
            new DataConversion(DataFormat.DibV5, DataFormat.Bitmap),
            new DataConversion(DataFormat.DibV5, DataFormat.Dib),
            new DataConversion(DataFormat.DibV5, DataFormat.Palette),
            new DataConversion(DataFormat.EnhMetaFile, DataFormat.MetaFilePict),
            new DataConversion(DataFormat.MetaFilePict, DataFormat.EnhMetaFile),
            new DataConversion(DataFormat.OemText, DataFormat.Text),
            new DataConversion(DataFormat.OemText, DataFormat.UnicodeText),
            new DataConversion(DataFormat.Text, DataFormat.OemText),
            new DataConversion(DataFormat.Text, DataFormat.UnicodeText),
            new DataConversion(DataFormat.UnicodeText, DataFormat.OemText),
            new DataConversion(DataFormat.UnicodeText, DataFormat.Text),
        });

        /// <summary>
        /// A list of data formats that are not expendable and
        /// should not be substituted for other type even if a 
        /// conversion can be made. Those are the formats that 
        /// should be saved intead of their substitutes.
        /// </summary>
        private static readonly IReadOnlyList<DataFormat> s_inexpendableFormats = new ReadOnlyCollection<DataFormat>(new DataFormat[]
        {
            DataFormat.Dib,
            DataFormat.UnicodeText,
            DataFormat.EnhMetaFile,
        });

        /// <summary>
        /// A list of the default synthesized (automatic) conversions that the 
        /// Windows OS can do by default. 
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms649013(v=vs.85).aspx#_win32_Synthesized_Clipboard_Formats"/>
        /// </summary>
        public static IReadOnlyList<DataConversion> DefaultConversions => s_defaultConversions;

        /// <summary>
        /// A list of data formats that are not expendable and
        /// should not be substituted for other type even if a 
        /// conversion can be made. Those are the formats that 
        /// should be saved intead of their substitutes.
        /// </summary>
        public static IReadOnlyList<DataFormat> InexpendableFormats => s_inexpendableFormats;

        /// <summary>
        /// Creates an array only of those DataFormats from the source collection, 
        /// that ought to be persisted, so that the integrity of the data can be preserved.
        /// </summary>
        /// <param name="source">An array of the source data formats.</param>
        public static DataFormat[] FilterInexpendable(DataFormat[] source)
        {
            List<DataFormat> result = new List<DataFormat>(source.Length);
            foreach (var format in source)
            {
                if (InexpendableFormats.Contains(format))
                {
                    result.Add(format);
                }
                else
                {
                    if (!result.Any(other => DefaultConversions.Contains(new DataConversion(other, format))))
                    {
                        result.Add(format);
                    }
                }
            }
            return result.ToArray();
        }

    }
}
