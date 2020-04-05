using System;
using System.Reflection.Emit;
using ILCompiler.Parser;
using ILCompiler.Tokenizer;

namespace ILCompiler
{
    public class Program
    {
        public delegate long CompileResult();
        
        public static CompileResult Compile(string sourceText)
        {
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), typeof(long), null);
            var generator = dynamicMethod.GetILGenerator();
            
            string[] programNames;
            string[] programExpressions;

            var programTokens = Tokenizers.Program.Tokenize(sourceText, out programNames, out programExpressions);
            
            var startNode = Parsers.Program.Parse(generator, programTokens, programNames, programExpressions);
            startNode.FinalGenerate(typeof(Program), generator);

            if (!startNode.FlowReturn)
            {
                throw new Exception("End of function is reachable without any return statement");
            }
            
            return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
        }

        public static long Index = 1;

        public static void Print(long a)
        {
            Console.WriteLine(a);
        }
        
        public static void Main(string[] args)
        {
            var sourceText = @"
decl a=1, b;
b = 1;
while ((Index + 1) < 8) {
    a = a + b;
    b = a - b;
    Index = Index + 1;
}

Print(a);

return (0 + a) - b;

";
            var result = Compile(sourceText);
            Console.WriteLine(result.Invoke());
            Console.WriteLine(Index);
        }
    }
}
