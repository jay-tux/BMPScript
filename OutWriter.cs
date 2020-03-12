using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace Jay.BMPScript
{
    public class OutWriter
    {
        private Queue<char> data = new Queue<char>();
        public void Write(string Data)
        {
            Array.ForEach(Data.ToCharArray(), x => data.Enqueue(x));
            Console.Write(Data);
        }

        public static void Debug(string Data) {}// => Console.Error.WriteLine(Data);

        /*public void Finish(int OutFile)
        {
            //byte[] data = new b
            int count = 0;
            int[] cols = new int[3];
            while(data.TryPeek(out string _))
            {
                col[count] = data.Dequeue();
                count++;
                if(count == 3)
                {
                    count = 0;
                    col.Enqueue(Color.FromArgb(col[0], col[1], col[2]));
                }
            }
            col.Enqueue(Color.FromArgb(col[0], col[1], col[2]));
            col.Enqueue(Color.Black);
        }*/

        /*private static void SaveBMP(byte[] buffer, int width, int height, string file)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            Rectangle BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // add back dummy bytes between lines, make each line be a multiple of 4 bytes
            int skipByte = bmpData.Stride - width*3;
            byte[] newBuff = new byte[buffer.Length + skipByte*height];
            for (int j = 0; j < height; j++)
            {
                Buffer.BlockCopy(buffer, j * width * 3, newBuff, j * (width * 3 + skipByte), width * 3);
            }

            // fill in rgbValues
            Marshal.Copy(newBuff, 0, ptr, newBuff.Length);
            b.UnlockBits(bmpData);
            b.Save(file, ImageFormat.Bmp);        
        }*/
    }
}