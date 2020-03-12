using System;
using System.IO;

namespace Jay.BMPScript
{
    public class ParseBMP
    {
        public CodeChar[,] Recreate(string BMP)
        {
            byte[] fData = GetBytes(BMP, out int Width, out int Height);
            CodeChar[,] data = new CodeChar[Width, Height];
            OutWriter.Debug($"[ BMPLOAD ]  Image is {Width}x{Height}.");
            for(int field = 0; field < fData.Length; field += 4)
            {
                int xPos = (field / 4) % Width;
                int yPos = Height - 1 - (field / 4) / Width;
                CodeChar pos = (CodeChar)(new byte[]{ fData[field + 2], fData[field + 1], fData[field] });
                OutWriter.Debug($"[ BMPLOAD ]    Filling @{xPos};{yPos}={pos}");
                data[xPos, yPos] = pos;
            }

            OutWriter.Debug("   ====  IMAGE OVERVIEW  ====   ");
            for(int y = 0; y < Height; y++)
            {
                string str = "";
                for(int x = 0; x < Width; x++)
                {
                    str += data[x, y].ToString() + " ";
                }
                OutWriter.Debug(str);
            }
            OutWriter.Debug("   ====  END OF OVERVIEW  ====   ");
            return data;
        }

        public byte[] GetBytes(string BMP, out int Width, out int Height)
        {
            byte[] bytes = File.ReadAllBytes(BMP);
            Width = GetField(18, 4, bytes);
            Height = GetField(22, 4, bytes);
            int start = GetField(10, 4, bytes);
            byte[] data = new byte[bytes.Length - start];
            int count = 1;
            for(int i = start; i < bytes.Length; i++)
            {
                data[i - start] = bytes[i];
                //Uncomment next two lines to display image byte-by-byte upon loading
                /*Console.Write(bytes[i].ToString("X2") + " ");
                if(count % 20 == 0) { Console.WriteLine(); }*/
                count++;
            }
            //Uncomment next line to fix formatting after showing image
            //Console.WriteLine();
            return data;
        }

        protected int GetField(int byteOffset, int len, byte[] array)
        {
            byte[] subArray = new byte[len];
            for(int i = 0; i < len; i++)
            {
                subArray[i] = array[i + byteOffset];
            }
            return BitConverter.ToInt32(subArray, 0);
        }
    }
}