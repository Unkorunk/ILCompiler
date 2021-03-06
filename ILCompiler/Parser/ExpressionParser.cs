﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
                    token == TokenExpression.Equal || token == TokenExpression.UnEqual ||
                    token == TokenExpression.LogicalAnd || token == TokenExpression.LogicalOr ||
                    token == TokenExpression.GreatEqual || token == TokenExpression.LessEqual);
        }

        private int GetPriority(TokenExpression token)
        {
            switch (token)
            {
                case TokenExpression.LogicalOr:
                    return 0;
                case TokenExpression.LogicalAnd:
                    return 1;
                case TokenExpression.Equal:
                case TokenExpression.UnEqual:
                    return 2;
                case TokenExpression.Great:
                case TokenExpression.Less:
                case TokenExpression.GreatEqual:
                case TokenExpression.LessEqual:
                    return 3;
                case TokenExpression.Add:
                case TokenExpression.Sub:
                    return 4;
                case TokenExpression.Div:
                case TokenExpression.Mul:
                    return 5;
                case TokenExpression.LogicalNot:
                    return 6;
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
                case TokenExpression.LogicalAnd: return Operation.LogicalAnd;
                case TokenExpression.LogicalOr: return Operation.LogicalOr;
                case TokenExpression.LogicalNot: return Operation.LogicalNot;
            }

            throw new Exception("unexpected token");
        }

        public ExpressionNode Parse(in TokenExpression[] tokens, in string[] names, in long[] constants, in Dictionary<string, LocalBuilder> declaredNames)
        {
            var constIndex = 0;
            var nameIndex = 0;
            
            var variableStack = new Stack<ExpressionNode>();
            var operationStack = new Stack<TokenExpression>();
            var functionStack = new Stack<(string, int)>();

            void ProcessAction()
            {
                var operation = operationStack.Pop();

                if (IsBinaryOperation(operation))
                {
                    if (variableStack.Count < 2) throw new Exception("invalid expression");
                    
                    var secondArg = variableStack.Pop();
                    var firstArg = variableStack.Pop();
                    
                    if (operation == TokenExpression.GreatEqual)
                    {
                        variableStack.Push(new OperationNode(Operation.LogicalOr, new ExpressionNode[]
                        {
                            new OperationNode(Operation.Great, new []{firstArg, secondArg}),
                            new OperationNode(Operation.Equal, new []{firstArg, secondArg}) 
                        }));
                    } else if (operation == TokenExpression.LessEqual)
                    {
                        variableStack.Push(new OperationNode(Operation.LogicalOr, new ExpressionNode[]
                        {
                            new OperationNode(Operation.Less, new []{firstArg, secondArg}),
                            new OperationNode(Operation.Equal, new []{firstArg, secondArg}) 
                        }));
                    }
                    else
                    {
                        variableStack.Push(new OperationNode(GetOperation(operation), new[] {firstArg, secondArg}));
                    }
                }
                else if (operation == TokenExpression.LogicalNot)
                {
                    if (variableStack.Count == 0) throw new Exception("invalid expression");
                    var firstArg = variableStack.Pop();
                    variableStack.Push(new OperationNode(GetOperation(operation), new[] {firstArg}));
                }
                else if (operation == TokenExpression.Function)
                {
                    var (functionName, expectedArgs) = functionStack.Pop();
                    if (variableStack.Count < expectedArgs) throw new Exception("invalid expression");

                    var args = new ExpressionNode[expectedArgs];
                    for (int i = expectedArgs - 1; i >= 0; i--)
                    {
                        args[i] = variableStack.Pop();
                    }
                    
                    variableStack.Push(new OperationNode(Operation.Function, args, functionName, typeof(long)));
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token == TokenExpression.Comma) continue;
                
                if (token == TokenExpression.Name)
                {
                    var name = names[nameIndex++];
                    if (name == "x")
                    {
                        variableStack.Push(new DataNode(true, 0));
                    }
                    else if (name == "y")
                    {
                        variableStack.Push(new DataNode(true, 1));
                    }
                    else if (name == "z")
                    {
                        variableStack.Push(new DataNode(true, 2));
                    }
                    else if (declaredNames.ContainsKey(name))
                    {
                        variableStack.Push(new DataNode(declaredNames[name]));
                    }
                    else if (i + 1 < tokens.Length && tokens[i + 1] != TokenExpression.Open)
                    {
                        variableStack.Push(new DataNode(name));
                    }
                    else
                    {
                        var expectedArgs = 1;
                        for (int j = i; j < tokens.Length; j++)
                        {
                            if (tokens[j] == TokenExpression.Close) break;
                            if (tokens[j] == TokenExpression.Comma) expectedArgs++;
                        }

                        if (i + 2 < tokens.Length && tokens[i + 2] == TokenExpression.Close) expectedArgs = 0;
                        
                        functionStack.Push((name, expectedArgs));
                        operationStack.Push(TokenExpression.Function);
                    }
                }
                else if (token == TokenExpression.Const)
                {
                    variableStack.Push(new DataNode(constants[constIndex++]));
                }
                else if (token == TokenExpression.LogicalNot)
                {
                    operationStack.Push(token);
                }
                else if (token == TokenExpression.Open)
                {
                    operationStack.Push(token);
                }
                else if (token == TokenExpression.Close)
                {
                    // TODO: create check on empty brackets
                    // if (operationStack.Peek() == TokenExpression.Open)
                    // {
                    //     throw new Exception("empty or not needed brackets");
                    // }
                    
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
                    while (operationStack.Count != 0 && (operationStack.Peek() == TokenExpression.Function ||
                                                         operationStack.Peek() == TokenExpression.LogicalNot ||
                                                         IsBinaryOperation(operationStack.Peek()) &&
                                                         GetPriority(operationStack.Peek()) >= GetPriority(token)))
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
