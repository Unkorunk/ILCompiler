using System;
using System.Reflection.Emit;
using ILCompiler.Parser;
using ILCompiler.Tokenizer;

namespace ILCompiler
{
    public class Compiler
    {
        public delegate long CompileResult();

        public static Type Scope = typeof(Program);
        
        public static CompileResult Compile(string sourceText)
        {
            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(), typeof(long), null);
            var generator = dynamicMethod.GetILGenerator();
            
            string[] programNames;
            string[] programExpressions;

            var programTokens = Tokenizers.Program.Tokenize(sourceText, out programNames, out programExpressions);
            
            var startNode = Parsers.Program.Parse(generator, programTokens, programNames, programExpressions);
            startNode.FinalGenerate(Scope, generator);

            if (!startNode.FlowReturn)
            {
                throw new Exception("End of function is reachable without any return statement");
            }
            
            return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
        }
    }
}