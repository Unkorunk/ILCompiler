using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace ILCompiler
{
    internal class Program
    {
        public delegate long CompileResult(long x, long y, long z);
        
        private static List<long> constStorage = new List<long>();

        public enum Token
        {
            X,
            Y,
            Z,
            Stack,
            Add,
            Sub,
            Mul,
            Div,
            Const
        }

        private static Token ToToken(string rawToken)
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
            }

            if (long.TryParse(rawToken, out _))
            {
                return Token.Const;
            }
            
            throw new Exception("unexpected symbol");
        }

        private static int GetPriority(Token token)
        {
            switch (token)
            {
                case Token.Add:
                case Token.Sub:
                    return 0;
                case Token.Div:
                case Token.Mul:
                    return 1;
                default:
                    throw new Exception("unexpected token");
            }
        }
        
        private static void EmitToken(in ILGenerator generator, Token token, int constIndex = -1)
        {
            switch (token)
            {
                case Token.X:
                    generator.Emit(OpCodes.Ldarg_0);
                    break;
                case Token.Y:
                    generator.Emit(OpCodes.Ldarg_1);
                    break;
                case Token.Z:
                    generator.Emit(OpCodes.Ldarg_2);
                    break;
                case Token.Add:
                    generator.Emit(OpCodes.Add);
                    break;
                case Token.Sub:
                    generator.Emit(OpCodes.Sub);
                    break;
                case Token.Mul:generator.Emit(OpCodes.Mul);
                    break;
                case Token.Div:
                    generator.Emit(OpCodes.Div);
                    break;
                case Token.Stack:
                    break;
                case Token.Const:
                    generator.Emit(OpCodes.Ldc_I8, constStorage[constIndex]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token), token, null);
            }
        }

        public static CompileResult Compile(String expression)
        {
            var rawTokens = expression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var tokens = new Token[rawTokens.Length];
            var constIndex = 0;
            for (var i = 0; i < rawTokens.Length; i++)
            {
                tokens[i] = ToToken(rawTokens[i]);
                if (tokens[i] == Token.Const)
                {
                    constStorage.Add(long.Parse(rawTokens[i]));
                }
            }

            var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(),
                typeof(long),
                new[] {typeof(long), typeof(long), typeof(long)});

            var generator = dynamicMethod.GetILGenerator();

            var variableStack = new Stack<Token>();
            var constStack = new Stack<int>();
            var operationStack = new Stack<Token>();

            foreach (var token in tokens)
            {
                if (token == Token.X || token == Token.Y || token == Token.Z)
                {
                    variableStack.Push(token);
                }
                else if (token == Token.Const)
                {
                    constStack.Push(constIndex++);
                    variableStack.Push(token);
                }
                else
                {
                    while (operationStack.Count != 0 && GetPriority(operationStack.Peek()) >= GetPriority(token))
                    {
                        var secondArg = variableStack.Pop();
                        var firstArg = variableStack.Pop();
                        var operation = operationStack.Pop();

                        var firstConstIndex = -1;
                        var secondConstIndex = -1;
                        if (firstArg == Token.Const)
                        {
                            firstConstIndex = constStack.Pop();
                        }
                        if (secondArg == Token.Const)
                        {
                            secondConstIndex = constStack.Pop();
                        }
                        
                        EmitToken(generator, firstArg, firstConstIndex);
                        EmitToken(generator, secondArg, secondConstIndex);
                        EmitToken(generator, operation);
                        variableStack.Push(Token.Stack);
                    }
                    operationStack.Push(token);
                }
            }
            
            while (operationStack.Count != 0)
            {
                var secondArg = variableStack.Pop();
                var firstArg = variableStack.Pop();
                var operation = operationStack.Pop();

                var firstConstIndex = -1;
                var secondConstIndex = -1;
                if (firstArg == Token.Const)
                {
                    firstConstIndex = constStack.Pop();
                }
                if (secondArg == Token.Const)
                {
                    secondConstIndex = constStack.Pop();
                }
                        
                EmitToken(generator, firstArg, firstConstIndex);
                EmitToken(generator, secondArg, secondConstIndex);
                EmitToken(generator, operation);
                variableStack.Push(Token.Stack);
            }
            
            generator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
        }

        public static void Main(string[] args)
        {
            var result = Compile("x * x + 2 * x + 1");

            Console.WriteLine(result.Invoke(4, 2, 1));
        }
    }
}
