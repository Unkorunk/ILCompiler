using System;
using ILCompiler.Parser;
using ILCompiler.Token;
using ILCompiler.Tokenizer;

namespace ILCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sourceText = @"
decl a=1, b, i=0;
b = 1;
while (i < 20) {
    a = a + b;
    b = a - b;
    i = i + 1;
}
return a;
";
            string[] programNames;
            string[] programExpressions;

            var programTokens = Tokenizers.Program.Tokenize(sourceText, out programNames, out programExpressions);

            foreach (var name in programNames)
            {
                Console.Write(name + " ");
            }
            Console.WriteLine();

            Console.WriteLine();
            foreach (var expression in programExpressions)
            {
                string[] exprNames;
                int exprNamesIdx = 0;
                long[] exprConstants;
                int exprConstantsIdx = 0;
                var exprTokens = Tokenizers.Expression.Tokenize(expression, out exprNames, out exprConstants);
                foreach (var exprToken in exprTokens)
                {
                    Console.Write(exprToken);
                    if (exprToken == TokenExpression.Const)
                    {
                        Console.Write("(" + exprConstants[exprConstantsIdx++] + ")");
                    } else if (exprToken == TokenExpression.Name)
                    {
                        Console.Write("(" + exprNames[exprNamesIdx++] + ")");
                    }
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            
            foreach (var token in programTokens)
            {
                Console.Write(token + " ");
            }
            Console.WriteLine();
            
            Parsers.Program.Parse(programTokens, programNames, programExpressions);
        }
    }
}
