using System;
using System.Drawing;
using System.IO;

namespace Jay.BMPScript
{
    public class CodeChar
    {
        public enum Order{}
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

        public static explicit operator int(CodeChar vl) =>
            (vl.Fields[0] * 65536 + vl.Fields[1] * 256 + vl.Fields[2]);

        public static explicit operator char(CodeChar vl) =>
            (char)((int)Math.Round(Math.Pow((int)vl, 1.0/3)));
        
        public static explicit operator string(CodeChar vl) =>
            ($"({(vl.Fields[0].ToString("X"))}, {(vl.Fields[0].ToString("X"))}, {(vl.Fields[0].ToString("X"))}) -> {Enum.GetNames(typeof(Order))[(Order)vl]}");

        public static explicit operator Order(CodeChar vl) =>
            (Order)((int)vl / Enum.GetNames(typeof(Order)).Length);
    }
}