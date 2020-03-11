using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Jay.IEnumerators;
using System.Windows.Forms;

namespace Jay.BMPScript
{
    public class Loader
    {
        private Color[,] Data;
        private Point Entry;

        public Loader(string Input) => Load(Input);
        public Loader(string Input, int Depth) => Load(Input, Depth);

        protected void Load(string Input, int Depth = 0)
        {
            if(Depth > 100) { return; }
            if(File.Exists(Input))
            {
                try
                {
                    Entry = new Point(0, 0);
                    Image i = Image.FromFile(Input);
                    OutWriter.Debug($"Trying to load Image: {i.GetLastStatus()}");
                    Bitmap img = new Bitmap(i);
                    //Bitmap img = new Bitmap(Input);
                    OutWriter.Debug("Image Loaded");
                    Data = new Color[img.Width, img.Height];
                    for(int x = 0; x < img.Width; x++)
                    {
                        for(int y = 0; y < img.Height; y++)
                        {
                            Data[x, y] = img.GetPixel(x, y);
                            if((CodeChar.Order)((CodeChar)Data[x, y]) == CodeChar.Order.Entry)
                            {
                                Entry = new Point(x, y);
                            }
                        }
                    }
                    new Parser(Data.ToCodeChar(), Depth).Start(Entry);
                }
                catch(ArgumentException ane)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    OutWriter.Debug($"Argument Exception: {Input}: {ane.Message}\n\t{ane.StackTrace}");
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