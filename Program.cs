using System;
using System.Drawing;
using System.IO;

namespace Jay.BMPScript
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length == 0) 
            { 
                Main(new string[] { Environment.CurrentDirectory + "/0.bmp" } ); 
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("No image to parse.");
            }
        }
    }
}