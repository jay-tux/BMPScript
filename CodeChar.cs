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
        
        private int[] Fields;
        private CodeChar(int[] vl) { Fields = vl; }

        public static implicit operator CodeChar(int vl)
        {
            if(vl > 16777216 || vl < 0) 
                { throw new BMPScriptException("core::readChar", "ValueError", "Value must be in range [0, 16 777 216]."); }
            int R = vl / (65536);
            int G = R / 256;
            int B = G % 256;
            return new CodeChar(new int[] { R, G, B });
        }

        public int GetField(Part field) => Fields[(int)field];

        public static implicit operator CodeChar(Color vl)
            => new CodeChar(new int[] { vl.R, vl.G, vl.B });

        public static explicit operator int(CodeChar vl) =>
            (vl.Fields[0] * 65536 + vl.Fields[1] * 256 + vl.Fields[2]);

        public static explicit operator CodeChar(byte[] vl)
            => new CodeChar(new int[]{ vl[0], vl[1], vl[2] });

        public static explicit operator Color(CodeChar vl)
            => Color.FromArgb(vl.Fields[0], vl.Fields[1], vl.Fields[2]);

        public static explicit operator char(CodeChar vl) =>
            (char)((int)Math.Round(Math.Pow((int)vl, 1.0/3)));
        
        public static explicit operator string(CodeChar vl) =>
            ($"({(vl.Fields[0].ToString("X2"))}, {(vl.Fields[1].ToString("X2"))}, {(vl.Fields[2].ToString("X2"))}) -> {Enum.GetNames(typeof(Order))[(int)((Order)vl)]}");

        public static explicit operator Order(CodeChar vl) =>
            (Order)((int)vl * Enum.GetNames(typeof(Order)).Length / 16777216);   
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