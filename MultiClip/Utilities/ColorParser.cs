using System;
using System.Linq;
using System.Windows.Media;

namespace MultiClip.Utilities
{
    public static class ColorParser
    {
        public static bool TryParse(string text, out Color color)
        {
            try
            {
                text = text.Replace(" ", "");
                if (text.EndsWith(")") && (text.StartsWith("rgb(") || text.StartsWith("rgba(")))
                {
                    int openParIdx = text.IndexOf("(");
                    string[] values = text.Substring(openParIdx + 1, text.Length - openParIdx - 2).Split(',');
                    if (values.Length == 3)
                    {
                        if (byte.TryParse(values[0], out byte r)
                            && byte.TryParse(values[1], out byte g)
                            && byte.TryParse(values[2], out byte b))
                        {
                            color = new Color { R = r, G = g, B = b };
                            return true;
                        }
                    }
                    else if (values.Length == 4)
                    {
                        if (byte.TryParse(values[0], out byte r)
                            && byte.TryParse(values[1], out byte g)
                            && byte.TryParse(values[2], out byte b)
                            && float.TryParse(values[3], out float a))
                        {
                            color = new Color { R = r, G = g, B = b, A = (byte)(a * 255) };
                            return true;
                        }
                    }
                }
                else if (text.All(x => char.IsLetter(x)) || (text.StartsWith("#") && text.Skip(1).All(x => char.IsLetterOrDigit(x))))
                {
                    color = System.Drawing.ColorTranslator.FromHtml(text)
                        .Let(x => new Color { A = x.A, R = x.R, G = x.G, B = x.B });
                    return true;
                }
            }
            catch (Exception)
            {
            }

            color = default;
            return false;
        }
    }
}
