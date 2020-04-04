using System;
using System.Reflection;
using System.Reflection.Emit;
using ILCompiler.Parser;
using ILCompiler.SyntaxTree;
using ILCompiler.Token;
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
            
            return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
        }
        
        public static void Main(string[] args)
        {
            var sourceText = @"
decl a=1, b, i=2;
b = 1;
while (i != 30) {
    a = a + b;
    b = a - b;
    i = i + 1;
}
return a;
";
            var result = Compile(sourceText);
            Console.WriteLine(result.Invoke());
        }
    }
}
