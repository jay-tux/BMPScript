using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Jay.IEnumerators;

namespace Jay.BMPScript
{
    public class Loader
    {
        private CodeChar[,] Data;
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
                    OutWriter.Debug("[ LOAD    ]Trying to load/convert image...");
                    Data = new ParseBMP().Recreate(Input);
                    if(Data == null) { OutWriter.Debug("[ LOAD ]  Failed to load."); return; }
                    OutWriter.Debug($"[ LOAD    ]    Recreated image in RAM ({Data.GetLength(0)}x{Data.GetLength(1)})");
                    for(int x = 0; x < Data.GetLength(0); x++)
                    {
                        for(int y = 0; y < Data.GetLength(1); y++)
                        {
                            if(Data[x, y] == null) { OutWriter.Debug($"[  LOAD ]      CodeChar@({x},{y}) is null."); }
                            if((CodeChar.Order)(Data[x, y]) == CodeChar.Order.Entry)
                            {
                                Entry = new Point(x, y);
                            }
                        }
                    }
                    OutWriter.Debug("[ LOAD    ]    Ready");
                    new Parser(Data, Depth).Start(Entry);
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