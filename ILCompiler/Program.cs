﻿using System;

namespace ILCompiler
{
    public class Program
    {
        public static void PrintLine(long x) { Console.WriteLine(x); }

        public static void ManyArgs(long a, long b, long c)
        {
            Console.WriteLine((a + b) * c);
        }
        public static long Add(long a, long b) => a + b;
        public static long Two() => 2;

        public static void Main(string[] args)
        {
            var result1 = Compiler.Compile(@"
                decl a, b, c;     
                ManyArgs(   32,    132,53);
                a = Add(x, y);
                c = Two();
                PrintLine(c);
                b = z - 12;
                if (a > b) {
                    PrintLine(42);
                } else {
                    PrintLine(112);
                    return a;
                }
                return 12;
            ");
            Console.WriteLine(result1.Invoke(1, 2, 3)); // prints 42, returns 12
            Console.WriteLine(result1.Invoke(1, 2, 100)); // prints 112, returns 3

            var result2 = Compiler.Compile(@"
                if (x > y) {
                    return 1;
                }
            ");
        }
    }
}
