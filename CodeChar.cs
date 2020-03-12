using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Jay.BMPScript
{
    public class CodeChar
    {
        public enum Order{ Entry, WriteV, WriteC, WriteLn, Label, If, Math, RNG, RGNV, Parse, Not, Jump, VarCP, Var, Read, Exit }
        public enum Part { R, G, B }
        
        private int R;
        private int G;
        private int B;
        
        private CodeChar(int R, int G, int B) { this.R = R; this.G = G; this.B = B; }

        public static implicit operator CodeChar(int vl)
        {
            if(vl > 16777216 || vl < 0) 
                { throw new BMPScriptException("core::readChar", "ValueError", "Value must be in range [0, 16 777 216]."); }
            int R = vl / (65536);
            int G = R / 256;
            int B = G % 256;
            return new CodeChar(R, G, B );
        }

        public int GetField(Part field) => (field == Part.R) ? R : (field == Part.G) ? G : B;

        public static implicit operator CodeChar(Color vl)
            => new CodeChar( vl.R, vl.G, vl.B );

        public static explicit operator int(CodeChar vl) =>
            (vl.R * 65536 + vl.G * 256 + vl.B);

        public static explicit operator CodeChar(byte[] vl)
            => new CodeChar( vl[0], vl[1], vl[2] );

        public static explicit operator Color(CodeChar vl)
            => Color.FromArgb(vl.R, vl.G, vl.B);

        public static explicit operator char(CodeChar vl) =>
            (char)((int)Math.Round(Math.Pow((int)vl, 1.0/3)));
        
        public static explicit operator string(CodeChar vl) =>
            ($"({(vl.R.ToString("X2"))}, {(vl.G.ToString("X2"))}, {(vl.B.ToString("X2"))}) -> {Enum.GetNames(typeof(Order))[(int)((Order)vl)]}");

        public static explicit operator Order(CodeChar vl) =>
            (Order)((int)vl * Enum.GetNames(typeof(Order)).Length / 16777216);   

        public override string ToString() => $"0x{R.ToString("X2")}{G.ToString("X2")}{B.ToString("X2")}";
    }

    public static class ConversionHelper
    {
        public static CodeChar[] ToCodeChar(this Color[] vl)
            => vl.ToList().Select(x => (CodeChar)x).ToArray();

        public static CodeChar[,] ToCodeChar(this Color[,] vl)
        {
            CodeChar[,] ret = new CodeChar[vl.GetLength(0), vl.GetLength(1)];
            for(int x = 0; x < vl.GetLength(0); x++)
            {
                for(int y = 0; y < vl.GetLength(1); y++)
                {
                    ret[x, y] = (CodeChar)vl[x, y];
                }
            }
            return ret;
        }
    }
}