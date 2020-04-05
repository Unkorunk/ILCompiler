using System;
using System.Reflection.Emit;
using ILCompiler.Parser;
using ILCompiler.Tokenizer;

namespace ILCompiler
{
    public class Compiler
    {
        public delegate long CompileResult(long x, long y, long z);

        public static Type Scope = typeof(Program);
        
        public static CompileResult Compile(string sourceText)
        {
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), typeof(long), 
                new []{typeof(long), typeof(long), typeof(long)});
            var generator = dynamicMethod.GetILGenerator();
            
            string[] programNames;
            string[] programExpressions;

            // try
            // {
                var programTokens = Tokenizers.Program.Tokenize(sourceText, out programNames, out programExpressions);

                var startNode = Parsers.Program.Parse(generator, programTokens, programNames, programExpressions);
                startNode.FinalGenerate(Scope, generator);

                if (!startNode.FlowReturn)
                {
                    throw new Exception("end of function is reachable without any return statement");
                }

                return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"[error] {ex.Message}");
            //     throw;
            // }
        }
    }
}