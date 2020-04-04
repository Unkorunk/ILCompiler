using System;
using System.Collections.Generic;
using ILCompiler.SyntaxTree;
using ILCompiler.Token;

namespace ILCompiler.Parser
{
    public class ExpressionParser
    {
        private bool IsBinaryOperation(TokenExpression token)
        {
            return (token == TokenExpression.Add || token == TokenExpression.Sub ||
                    token == TokenExpression.Mul || token == TokenExpression.Div ||
                    token == TokenExpression.Great || token == TokenExpression.Less ||
                    token == TokenExpression.Equal || token == TokenExpression.UnEqual);
        }

        private int GetPriority(TokenExpression token)
        {
            switch (token)
            {
                case TokenExpression.Equal:
                case TokenExpression.UnEqual:
                    return 0;
                case TokenExpression.Great:
                case TokenExpression.Less:
                    return 1;
                case TokenExpression.Add:
                case TokenExpression.Sub:
                    return 2;
                case TokenExpression.Div:
                case TokenExpression.Mul:
                    return 3;
                default:
                    throw new Exception("unexpected token");
            }
        }

        private Operation GetOperation(TokenExpression token)
        {
            switch (token)
            {
                case TokenExpression.Add: return Operation.Add;
                case TokenExpression.Sub: return Operation.Sub;
                case TokenExpression.Mul: return Operation.Mul;
                case TokenExpression.Div: return Operation.Div;
                case TokenExpression.Great: return Operation.Great;
                case TokenExpression.Less: return Operation.Less;
                case TokenExpression.Equal: return Operation.Equal;
                case TokenExpression.UnEqual: return Operation.UnEqual;
            }

            throw new Exception("unexpected token");
        }

        public ExpressionNode Parse(in TokenExpression[] tokens, in string[] names, in long[] constants, in List<string> declaredNames)
        {
            var constIndex = 0;
            var nameIndex = 0;
            
            var variableStack = new Stack<ExpressionNode>();
            var operationStack = new Stack<TokenExpression>();

            void ProcessAction()
            {
                var secondArg = variableStack.Pop();
                var firstArg = variableStack.Pop();
                var operation = operationStack.Pop();

                if (IsBinaryOperation(operation))
                {
                    variableStack.Push(new OperationNode(GetOperation(operation), new[] {firstArg, secondArg}));
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            foreach (var token in tokens)
            {
                if (token == TokenExpression.Name)
                {
                    var name = names[nameIndex++];
                    if (declaredNames.Contains(name))
                    {
                        variableStack.Push(new DataNode(declaredNames.IndexOf(name)));
                    }
                    else
                    {
                        variableStack.Push(new DataNode(name));
                    }
                }
                else if (token == TokenExpression.Const)
                {
                    variableStack.Push(new DataNode(constants[constIndex++]));
                }
                else if (token == TokenExpression.Open)
                {
                    operationStack.Push(token);
                }
                else if (token == TokenExpression.Close)
                {
                    if (operationStack.Peek() == TokenExpression.Open)
                    {
                        throw new Exception("empty brackets");
                    }
                    
                    while (operationStack.Count != 0 && operationStack.Peek() != TokenExpression.Open)
                    {
                        ProcessAction();
                    }

                    if (operationStack.Peek() == TokenExpression.Open)
                    {
                        operationStack.Pop();
                    }
                    else
                    {
                        throw new Exception("unbalanced brackets");
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
                if (operationStack.Peek() == TokenExpression.Open || operationStack.Peek() == TokenExpression.Close)
                {
                    throw new Exception("unbalanced brackets");
                }
                
                ProcessAction();
            }

            if (variableStack.Count != 1)
            {
                throw new Exception("invalid expression");
            }
            
            return variableStack.Pop();
        }
    }
}
