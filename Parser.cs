using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Jay.IEnumerators;
using System.Linq;

namespace Jay.BMPScript
{
    public class Parser
    {
        public static Random RNG = new Random();
        private Dictionary<CodeChar, Point> Labels;
        private Dictionary<int, int> Integers;
        private Dictionary<int, char> Characters;
        private CodeChar[,] Program;
        private int Read = 0;
        private int Write = 1;
        private int Depth;

        public Parser(CodeChar[,] Program, int Depth)
        {
            this.Program = Program;
            this.Depth = Depth;
        }

        protected CodeChar GetAt(Point Pos) => Program[Pos.X, Pos.Y];

        protected void SysDump()
        {
            OutWriter.Debug("   ====  Full SYS_DUMP  ====   ");
            OutWriter.Debug(" => Defined Labels:");
            Labels.Select(x => $"\t{x.Key}: ({x.Value.X}, {x.Value.Y})").ToList().ForEach(x => OutWriter.Debug(x));
            OutWriter.Debug(" ---- ---- ");
            OutWriter.Debug(" => Defined Variables:");
            OutWriter.Debug("   -> Defined Integers:");
            Integers.Select(x => $"\t{x.Key.ToString("D2")}: {x.Value}").ToList().ForEach(x => OutWriter.Debug(x));
            OutWriter.Debug("  --    --  ");
            OutWriter.Debug("    -> Defined Characters:");
            Characters.Select(x => $"\t{x.Key.ToString("D2")}: {x.Value}").ToList().ForEach(x => OutWriter.Debug(x));
            OutWriter.Debug("   ==== End of SYS_DUMP ====   ");
        }

        protected void Overview(Point Entry)
        {
            OutWriter.Debug("  == Program Overview ==  ");
            IEnumerator<Point> it = new Iteration2D(Entry.X, Entry.Y, Program.GetLength(0), Program.GetLength(1)).Snake(270, true);
            while(it.MoveNext())
            {
                OutWriter.Debug("    " + GetAt(it.Current).ToString());
            }
            OutWriter.Debug("  == End of Overview ==  ");
        }

        protected void PreProcess(Point Entry)
        {
            Overview(Entry);
            OutWriter.Debug("  Started Preprocessor...");
            IEnumerator<Point> it = new Iteration2D(Entry.X, Entry.Y, Program.GetLength(0), Program.GetLength(1))
                .Snake(270, true);
            while(it.MoveNext())
            {
                OutWriter.Debug($"    Current Iterator value: {it.Current}");
                //Jump to labels only
                switch((CodeChar.Order)GetAt(it.Current))
                {
                    //Skip none
                    case CodeChar.Order.Entry:
                    case CodeChar.Order.WriteLn:
                    case CodeChar.Order.Parse:
                    case CodeChar.Order.Exit:
                        break;
                    
                    //Skip two
                    case CodeChar.Order.If:
                    case CodeChar.Order.Math:
                    case CodeChar.Order.Not:
                        it.MoveNext(); it.MoveNext();
                        break;
                    
                    //Label: mark
                    case CodeChar.Order.Label:
                        it.MoveNext();
                        Labels[GetAt(it.Current)] = new Point(it.Current.X, it.Current.Y);
                        OutWriter.Debug($"      Encountered Mark Label: ({GetAt(it.Current).ToString()}) := {Labels[GetAt(it.Current)].ToString()}");
                        break;

                    //Skip one
                    default:
                        it.MoveNext();
                        break;
                }
            }
            Console.WriteLine("    Labels:");
            Labels.Select(x => $"{x.Key}: ({x.Value.X}, {x.Value.Y})").ToList().ForEach(x => Console.WriteLine($"\t{x}"));
            OutWriter.Debug("  Preprocessor Ready.");
        }

        public void Start(Point Entry)
        {
            OutWriter.Debug("Starting Parser.");
            Labels = new Dictionary<CodeChar, Point>();
            Integers = new Dictionary<int, int>();
            Characters = new Dictionary<int, char>();
            if(Program == null) { Console.Error.Write("Program is empty."); }
            PreProcess(Entry);
            Iteration2D i2d = new Iteration2D(Entry.X, Entry.Y, Program.GetLength(0), Program.GetLength(1));
            bool fin = false;
            CodeChar cc;
            IEnumerator<Point> it = i2d.Snake(270);
            while(!fin && it.MoveNext())
            {
                OutWriter.Debug((string)GetAt(it.Current) + " ");
                switch((CodeChar.Order)GetAt(it.Current))
                {
                    case CodeChar.Order.Entry:  
                        continue;

                    case CodeChar.Order.Exit:   
                        fin = true;     
                        break;

                    case CodeChar.Order.If:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        if(EvaluateCheck(cc.GetField(CodeChar.Part.R), cc.GetField(CodeChar.Part.G), cc.GetField(CodeChar.Part.B)))
                        {
                            OutWriter.Debug("  Check Succeeded");
                            it.MoveNext();
                            if(Labels.ContainsKey(GetAt(it.Current)))
                            {
                                i2d.XPos = Labels[GetAt(it.Current)].X;
                                i2d.YPos = Labels[GetAt(it.Current)].Y;
                            }
                        }
                        else
                        {
                            OutWriter.Debug("  Check Failed");
                            it.MoveNext();
                        }
                        break;

                    case CodeChar.Order.Jump: 
                        it.MoveNext();  
                        if(Labels.ContainsKey(GetAt(it.Current)))
                        {
                            i2d.XPos = Labels[GetAt(it.Current)].X;
                            i2d.YPos = Labels[GetAt(it.Current)].Y;
                        }
                        break;

                    case CodeChar.Order.Label:
                        break;

                    case CodeChar.Order.Math:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        it.MoveNext();
                        Integers[GetAt(it.Current).GetField(CodeChar.Part.R)] = 
                            EvaluateMath(cc.GetField(CodeChar.Part.R), cc.GetField(CodeChar.Part.G), cc.GetField(CodeChar.Part.B));
                        break;

                    case CodeChar.Order.Not:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        if(!EvaluateCheck(cc.GetField(CodeChar.Part.R), cc.GetField(CodeChar.Part.G), cc.GetField(CodeChar.Part.B)))
                        {
                            OutWriter.Debug("  Check Succeeded");
                            it.MoveNext();
                            if(Labels.ContainsKey(GetAt(it.Current)))
                            {
                                i2d.XPos = Labels[GetAt(it.Current)].X;
                                i2d.YPos = Labels[GetAt(it.Current)].Y;
                            }
                        }
                        else
                        {
                            OutWriter.Debug("  Check Failed");
                            it.MoveNext();
                        }
                        break;

                    case CodeChar.Order.Parse:
                        Read++;
                        new Loader(Environment.CurrentDirectory + $"/{Read}.bmp", Depth);
                        break;

                    case CodeChar.Order.Read:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        if(cc.GetField(CodeChar.Part.R) >= 128)
                        {
                            string i = Console.ReadLine();
                            while(!int.TryParse(i, out int tmp)) { Console.Error.Write("Input error: Expected an integer: "); i = Console.ReadLine(); }
                            Integers[cc.GetField(CodeChar.Part.G)] = tmp;
                        }
                        else
                        {
                            string i = Console.ReadLine();
                            while(i.Length < 1) { Console.Error.Write("Input error: Expected a character: "); i = Console.ReadLine(); }
                            Characters[cc.GetField(CodeChar.Part.B)] = i[0];
                        }
                        break;

                    case CodeChar.Order.RNG:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        Integers[cc.GetField(CodeChar.Part.R)] = RNG.Next(
                                (cc.GetField(CodeChar.Part.G) < cc.GetField(CodeChar.Part.B) ? cc.GetField(CodeChar.Part.G) : cc.GetField(CodeChar.Part.B)),
                                (cc.GetField(CodeChar.Part.G) > cc.GetField(CodeChar.Part.B) ? cc.GetField(CodeChar.Part.G) : cc.GetField(CodeChar.Part.B))
                        );
                        break;

                    case CodeChar.Order.RGNV:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        int e1 = (Integers.ContainsKey(cc.GetField(CodeChar.Part.G)) ? Integers[cc.GetField(CodeChar.Part.G)] : 
                                Characters.ContainsKey(cc.GetField(CodeChar.Part.G)) ? (int)Characters[cc.GetField(CodeChar.Part.G)] : 
                                cc.GetField(CodeChar.Part.G));
                        int e2 = (Integers.ContainsKey(cc.GetField(CodeChar.Part.B)) ? Integers[cc.GetField(CodeChar.Part.B)] : 
                                Characters.ContainsKey(cc.GetField(CodeChar.Part.B)) ? (int)Characters[cc.GetField(CodeChar.Part.B)] : 
                                cc.GetField(CodeChar.Part.B));
                        Integers[cc.GetField(CodeChar.Part.R)] = RNG.Next((e1 < e2) ? e1 : e2, (e1 > e2) ? e2 : e1);
                        break;

                    case CodeChar.Order.Var:    
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        if(cc.GetField(CodeChar.Part.R) >= 128)
                        {
                            Integers[cc.GetField(CodeChar.Part.G)] = cc.GetField(CodeChar.Part.B);
                        }
                        else
                        {
                            Characters[cc.GetField(CodeChar.Part.G)] = (char)cc.GetField(CodeChar.Part.B);
                        }
                        break;

                    case CodeChar.Order.VarCP:
                        it.MoveNext();
                        cc = GetAt(it.Current);
                        if(cc.GetField(CodeChar.Part.R) >= 128)
                        {
                            Integers[cc.GetField(CodeChar.Part.G)] = 
                                (Integers.ContainsKey(cc.GetField(CodeChar.Part.B)) ? Integers[cc.GetField(CodeChar.Part.B)] : 
                                (Characters.ContainsKey(cc.GetField(CodeChar.Part.B)) ? (int)Characters[cc.GetField(CodeChar.Part.B)] : 
                                cc.GetField(CodeChar.Part.B)));
                        }
                        else
                        {
                            Characters[cc.GetField(CodeChar.Part.G)] = 
                                (Integers.ContainsKey(cc.GetField(CodeChar.Part.B)) ? (char)Integers[cc.GetField(CodeChar.Part.B)] : 
                                (Characters.ContainsKey(cc.GetField(CodeChar.Part.B)) ? Characters[cc.GetField(CodeChar.Part.B)] : 
                                (char)cc.GetField(CodeChar.Part.B)));
                        }
                        break;

                    case CodeChar.Order.WriteC: 
                        it.MoveNext();
                        cc = this.GetAt(it.Current);
                        Console.Write($"{(char)cc.GetField(CodeChar.Part.R)}{(char)cc.GetField(CodeChar.Part.G)}{(char)cc.GetField(CodeChar.Part.B)}");
                        break;

                    case CodeChar.Order.WriteLn:
                        Console.WriteLine();
                        break;

                    case CodeChar.Order.WriteV: 
                        it.MoveNext();
                        int vl = this.GetAt(it.Current).GetField(CodeChar.Part.R);
                        Console.Write((Integers.ContainsKey(vl) ? Integers[vl] : Characters.ContainsKey(vl) ? Characters[vl] : vl));
                        break;
                }
            }
            SysDump();
        }

        protected int EvaluateMath(int ID1, int OP, int ID2)
        {
            int val1 = (Integers.ContainsKey(ID1) ? Integers[ID1] : (Characters.ContainsKey(ID1) ? (int)Characters[ID1] : ID1));
            int val2 = (Integers.ContainsKey(ID2) ? Integers[ID2] : (Characters.ContainsKey(ID2) ? (int)Characters[ID2] : ID2));
            return (OP < 64) ? val1 / val2 : 
                (OP < 128) ? val1 - val2 : 
                (OP < 192) ? val1 + val2 : val1 * val2;
        }

        protected bool EvaluateCheck(int ID1, int OP, int ID2)
        {
            int val1 = (Integers.ContainsKey(ID1) ? Integers[ID1] : (Characters.ContainsKey(ID1) ? (int)Characters[ID1] : ID1));
            int val2 = (Integers.ContainsKey(ID2) ? Integers[ID2] : (Characters.ContainsKey(ID2) ? (int)Characters[ID2] : ID2));
            return (OP < 64) ? val1 < val2 : 
                (OP < 128) ? val1 == val2 : 
                (OP < 192) ? val1 != val2 : val1 > val2;
        }
    }
}