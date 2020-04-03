using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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
            Const,
            Open,
            Close
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
                case "(": return Token.Open;
                case ")": return Token.Close;
            }

            if (long.TryParse(rawToken, out _))
            {
                return Token.Const;
            }
            
            throw new Exception("unexpected symbol");
        }

        private static bool IsBinaryOperation(Token token)
        {
            return (token == Token.Add || token == Token.Sub || token == Token.Mul || token == Token.Div);;
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

            void ProcessAction()
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
                else if (token == Token.Open)
                {
                    operationStack.Push(token);
                }
                else if (token == Token.Close)
                {
                    while (operationStack.Count != 0 && operationStack.Peek() != Token.Open)
                    {
                        ProcessAction();
                    }

                    if (operationStack.Peek() == Token.Open)
                    {
                        operationStack.Pop();
                    }
                    else
                    {
                        throw new Exception("unbalance brackets");
                    }
                }
                else if (IsBinaryOperation(token))
                {
                    while (operationStack.Count != 0 && 
                           IsBinaryOperation(operationStack.Peek()) && GetPriority(operationStack.Peek()) >= GetPriority(token))
                    {
                        ProcessAction();
                    }
                    operationStack.Push(token);
                }
                else
                {
                    throw new Exception("unexpected token");
                }
            }
            
            while (operationStack.Count != 0)
            {
                if (operationStack.Peek() == Token.Open || operationStack.Peek() == Token.Close)
                {
                    throw new Exception("unbalance brackets");
                }
                
                ProcessAction();
            }
            
            generator.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(CompileResult)) as CompileResult;
        }

        public static void Main(string[] args)
        {
            var result = Compile("x * ( 1 - y ) + z * z / 4");
            Console.WriteLine(result.Invoke(1, 1, 1)); // prints 0
            Console.WriteLine(result.Invoke(2, 2, 2)); // prints -1
            Console.WriteLine(result.Invoke(2, 3, 4)); // prints 0
            Console.WriteLine(result.Invoke(1, 0, 2)); // prints 2
        }
    }
}
