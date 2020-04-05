using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ILCompiler.SyntaxTree;
using ILCompiler.Token;
using ILCompiler.Tokenizer;

namespace ILCompiler.Parser
{
    public class ProgramParser
    {
        // program = [ "decl", ["name", ["assign", expr], { "comma", "name", ["assign", expr] }] ], body
        // body = { [ "name", ("assign", expr, "sem") |
        //                    ("ropen", [ "expr", { "comma", "expr" } ], "rclose", "sem") ] |
        //          [ "if", "ropen", expr, "rclose", "copen", body, "cclose", ["else", "copen", body, "cclose"]] |
        //          [ "while, "ropen", expr, "rclose", "copen", body, "cclose" ] |
        //          [ "return", "name", "sem" ] }
        
        private TokenProgram[] _tokens;
        private int _tokenIdx;

        private string[] _names;
        private int _namesIdx;

        private string[] _expressions;
        private int _expressionIdx;

        private readonly Dictionary<string, LocalBuilder> _declaredNames = new Dictionary<string, LocalBuilder>();

        private void AddNode(Node node, ref Node startNode, ref Node parentNode)
        {
            if (startNode == null)
            {
                startNode = node;
                parentNode = node;
            }
            else
            {
                parentNode.NextNode = node;
                parentNode = parentNode.NextNode;
            }
        }

        private bool Check(TokenProgram tokenProgram)
        {
            return _tokenIdx < _tokens.Length && _tokens[_tokenIdx] == tokenProgram;
        }
        
        private bool Accept(TokenProgram tokenProgram)
        {
            if (Check(tokenProgram))
            {
                _tokenIdx++;
                return true;
            }

            return false;
        }

        private void Expect(TokenProgram tokenProgram)
        {
            if (Accept(tokenProgram))
            {
                return;
            }

            throw new Exception("unexpected token");
        }

        private bool EndOfFile()
        {
            return _tokenIdx >= _tokens.Length;
        } 

        private ExpressionNode Expr()
        {
            Expect(TokenProgram.Expr);

            var exprTokens =
                Tokenizers.Expression.Tokenize(_expressions[_expressionIdx++], out var exprNames,
                    out var exprConstants);

            return Parsers.Expression.Parse(exprTokens, exprNames, exprConstants, _declaredNames);
        }

        private DataNode GetDataNode(string name)
        {
            return _declaredNames.ContainsKey(name) ? new DataNode(_declaredNames[name]) : new DataNode(name);
        }

        private Node Body()
        {
            Node startNode = null, parentNode = null;
            while (true)
            {
                if (Accept(TokenProgram.Name))
                {
                    var name = _names[_namesIdx++];
                    if (Accept(TokenProgram.Assign))
                    {
                        var expressionNode = Expr();
                        Expect(TokenProgram.Sem);
                        var assignNode = new AssignNode(GetDataNode(name), expressionNode);
                        AddNode(assignNode, ref startNode, ref parentNode);
                    }
                    else
                    {
                        Expect(TokenProgram.ROpen);

                        var listExpressions = new List<ExpressionNode>();
                        if (Check(TokenProgram.Expr))
                        {
                            listExpressions.Add(Expr());
                            while (Accept(TokenProgram.Comma))
                            {
                                listExpressions.Add(Expr());
                            }
                        }
                        
                        Expect(TokenProgram.RClose);
                        Expect(TokenProgram.Sem);
                        
                        listExpressions.Reverse();
                        var functionNode = new OperationNode(Operation.Function, listExpressions.ToArray(), name, typeof(void));
                        AddNode(functionNode, ref startNode, ref parentNode);
                    }
                }
                else if (Accept(TokenProgram.If))
                {
                    Expect(TokenProgram.ROpen);
                    var expressionNode = Expr();
                    Expect(TokenProgram.RClose);
                    Expect(TokenProgram.COpen);
                    var thenNode = Body();
                    Expect(TokenProgram.CClose);
                    Node elseNode = null;
                    if (Accept(TokenProgram.Else))
                    {
                        Expect(TokenProgram.COpen);
                        elseNode = Body();
                        Expect(TokenProgram.CClose);
                    }

                    var ifNode = new IfNode(expressionNode, thenNode, elseNode);
                    AddNode(ifNode, ref startNode, ref parentNode);
                }
                else if (Accept(TokenProgram.While))
                {
                    Expect(TokenProgram.ROpen);
                    var expressionNode = Expr();
                    Expect(TokenProgram.RClose);
                    Expect(TokenProgram.COpen);
                    var insideNode = Body();
                    Expect(TokenProgram.CClose);
                    var whileNode = new WhileNode(expressionNode, insideNode);
                    AddNode(whileNode, ref startNode, ref parentNode);
                }
                else if (Accept(TokenProgram.Return))
                {
                    Expect(TokenProgram.Name);
                    var name3 = _names[_namesIdx++];
                    var returnNode = new ReturnNode(GetDataNode(name3));
                    AddNode(returnNode, ref startNode, ref parentNode);
                    Expect(TokenProgram.Sem);
                }
                else
                {
                    break;
                }
            }

            return startNode;
        }

        private Node Program(in ILGenerator generator)
        {
            Node startNode = null, parentNode = null;

            if (Accept(TokenProgram.Decl))
            {
                if (Accept(TokenProgram.Name))
                {
                    var name1 = _names[_namesIdx++];
                    if (_declaredNames.ContainsKey(name1)) throw new Exception("redeclaration variable");
                    _declaredNames.Add(name1, generator.DeclareLocal(typeof(long)));
                    if (Accept(TokenProgram.Assign))
                    {
                        var expressionNode = Expr();
                        var assignNode = new AssignNode(GetDataNode(name1), expressionNode);
                        AddNode(assignNode, ref startNode, ref parentNode);
                    }

                    while (Accept(TokenProgram.Comma))
                    {
                        Expect(TokenProgram.Name);
                        var name2 = _names[_namesIdx++];
                        if (_declaredNames.ContainsKey(name2)) throw new Exception("redeclaration variable");
                        _declaredNames.Add(name2, generator.DeclareLocal(typeof(long)));
                        if (Accept(TokenProgram.Assign))
                        {
                            var expressionNode = Expr();
                            var assignNode = new AssignNode(GetDataNode(name2), expressionNode);
                            AddNode(assignNode, ref startNode, ref parentNode);
                        }
                    }
                }

                Expect(TokenProgram.Sem);
            }

            AddNode(Body(), ref startNode, ref parentNode);
            while (parentNode.NextNode != null)
            {
                parentNode = parentNode.NextNode;
            }
            
            parentNode.NextNode = new ReturnNode(new DataNode(-1), false);

            return startNode;
        }

        public Node Parse(in ILGenerator generator, in TokenProgram[] tokens, in string[] names,
            in string[] expressions)
        {
            _declaredNames.Clear();

            _tokens = tokens;
            _tokenIdx = 0;

            _names = names;
            _namesIdx = 0;

            _expressions = expressions;
            _expressionIdx = 0;
            
            var startNode = Program(generator);
            if (!EndOfFile()) throw new Exception("unexpected token");
            
            return startNode;
        }
    }
}
