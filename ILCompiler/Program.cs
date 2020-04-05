using System;

namespace ILCompiler
{
    public class Program
    {
        public static long Index = 1;

        public static void Print(long a)
        {
            Console.WriteLine(a);
        }
        
        public static void Main(string[] args)
        {
            var sourceText = @"
decl a = 1, b = 1, i = 0;
while (i < 20) {
    a = a + b;
    b = a - b;
    i = i + 1;
    Print(a);
}
return a;
";
            var result = Compiler.Compile(sourceText);
            result?.Invoke();
        }
    }
}
