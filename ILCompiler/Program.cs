using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace ILCompiler
{
    internal class Program
    {
        public delegate long CompileResult(long x, long y, long z);

        public enum Token
        {
            X,
            Y,
            Z,
            Add,
            Sub,
            Mul,
            Div
        }

        public static Token toToken(String rawToken)
        {
            switch (rawToken)
            {
                case "x": return Token.X;
                case "y": return Token.Y;
                case "z": return Token.Z;
                case "+": return Token.Add;
                case "-": return Token.Sub;
                case "*": return Token.Mul;
                case "/": return Token.Div;
                default: throw new Exception("unexpected symbol");
            }
        }

        public static CompileResult Compile(String expression)
        {
            var rawTokens = expression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var tokens = new Token[rawTokens.Length];
            for (var i = 0; i < rawTokens.Length; i++)
            {
                tokens[i] = toToken(rawTokens[i]);
            }

            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(),
                typeof(long),
                new[] {typeof(long), typeof(long), typeof(long)});

            var generator = dynamicMethod.GetILGenerator();



            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Add);

            generator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
        }

        public static void Main(string[] args)
        {
            var result = Compile("x    + y / z");

            Console.WriteLine(result.Invoke(1, 2, 3));
        }
    }
}
