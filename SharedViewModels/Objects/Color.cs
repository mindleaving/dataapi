using System;
using System.Globalization;

namespace SharedViewModels.Objects
{
    public class Color
    {
        public Color(string colorCode)
        {
            var code = colorCode.StartsWith("#")
                ? colorCode.Substring(1)
                : colorCode;
            if (code.Length == 3)
            {
                A = byte.MaxValue;
                R = byte.Parse($"{code[0]}{code[0]}", NumberStyles.HexNumber);
                G = byte.Parse($"{code[1]}{code[1]}", NumberStyles.HexNumber);
                B = byte.Parse($"{code[2]}{code[2]}", NumberStyles.HexNumber);
            }
            else if (code.Length == 6)
            {
                A = byte.MaxValue;
                R = byte.Parse($"{code[0]}{code[1]}", NumberStyles.HexNumber);
                G = byte.Parse($"{code[2]}{code[3]}", NumberStyles.HexNumber);
                B = byte.Parse($"{code[4]}{code[5]}", NumberStyles.HexNumber);
            }
            else if (code.Length == 8)
            {
                A = byte.Parse($"{code[0]}{code[1]}", NumberStyles.HexNumber);
                R = byte.Parse($"{code[2]}{code[3]}", NumberStyles.HexNumber);
                G = byte.Parse($"{code[4]}{code[5]}", NumberStyles.HexNumber);
                B = byte.Parse($"{code[6]}{code[7]}", NumberStyles.HexNumber);
            }
            else
            {
                throw new FormatException($"Unknown color code '{colorCode}'");
            }
        }
        public Color(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
    }
}
