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
            for(int field = 0; field < fData.Length; field += 4)
            {
                int xPos = (field / 4) % Width;
                int yPos = Height - 1 - (field / 4) / Width;
                CodeChar pos = (CodeChar)(new byte[]{ fData[field + 2], fData[field + 1], fData[field] });
                data[xPos, yPos] = pos;
            }

            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    Console.Write(data[x, y].ToString() + " ");
                }
                Console.WriteLine();
            }
            return data;
        }

        public byte[] GetBytes(string BMP, out int Width, out int Height)
        {
            byte[] bytes = File.ReadAllBytes(BMP);
            Width = GetField(18, 4, bytes);
            Height = GetField(22, 4, bytes);
            int start = GetField(10, 4, bytes);
            byte[] data = new byte[bytes.Length - start];
            for(int i = start; i < bytes.Length; i++)
            {
                data[i - start] = bytes[i];
            }
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