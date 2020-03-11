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
                Console.WriteLine($"Trying to load {args[0]}.");
                if(File.Exists(args[0]))
                {
                    Console.WriteLine($"Started parser on {args[0]}.");
                    new Loader(args[0]);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("No image to parse.");
                }
            }
        }
    }
}