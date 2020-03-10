using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Jay.IEnumerators;

namespace Jay.BMPScript
{
    public class Loader
    {
        private Color[] Data;
        private int Index;

        public Loader(string Input)
        {
            if(File.Exists(Input))
            {
                try
                {
                    Bitmap img = new Bitmap(Input);
                    Data = new Color[img.Height * img.Width];
                }
                catch(IOException ioe)
                {
                    throw new BMPScriptException("preproc::loader", "FileError", $"Can't read file {Input}.");
                }
            }
            else
            {
                throw new BMPScriptException("preproc::loader", "FileError", $"Script/BMP file {Input} does not exist.");
            }
        }
    }
}