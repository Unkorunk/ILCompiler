using System;
using System.Collections.Generic;
using ILCompiler.SyntaxTree;
using ILCompiler.Token;
using ILCompiler.Tokenizer;

namespace ILCompiler.Parser
{
    public class ProgramParser
    {
        // program = [ "decl", ["name", ["assign", expr]], { "comma", "name", ["assign", expr] } ], body, "return", "name", "sem"
        // body = { [ "name", "assign", expr, "sem" ] |
        //          [ "if", "ropen", expr, "rclose", "copen", body, "cclose", ["else", "copen", body, "cclose"]]
        //          [ "while, "ropen", expr, "rclose", "copen", body, "cclose" ] }


        private TokenProgram[] _tokens;
        private int _tokenIdx = 0;
        
        private string[] _names;
        private int _namesIdx = 0;
        
        private string[] _expressions;
        private int _expressionIdx = 0;

        private readonly List<string> _declaredNames = new List<string>();

        private bool Accept(TokenProgram tokenProgram)
        {
            if (_tokens[_tokenIdx] == tokenProgram)
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
            return _declaredNames.Contains(name) ? new DataNode(_declaredNames.IndexOf(name)) : new DataNode(name);
        }
        
        private Node Body()
        {
            while (true)
            {
                if (Accept(TokenProgram.Name))
                {
                    var name = _names[_namesIdx++];
                    Expect(TokenProgram.Assign);
                    var expressionNode = Expr();
                    Expect(TokenProgram.Sem);
                    var assignNode = new AssignNode(GetDataNode(name), expressionNode);
                } else if (Accept(TokenProgram.If))
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
                } else if (Accept(TokenProgram.While))
                {
                    Expect(TokenProgram.ROpen);
                    var expressionNode = Expr();
                    Expect(TokenProgram.RClose);
                    Expect(TokenProgram.COpen);
                    var insideNode = Body();
                    Expect(TokenProgram.CClose);
                    var whileNode = new WhileNode(expressionNode, insideNode);
                }
                else
                {
                    break;
                }
            }

            return null;
        }

        private void Program()
        {
            if (Accept(TokenProgram.Decl))
            {
                if (Accept(TokenProgram.Name))
                {
                    var name1 = _names[_namesIdx++];
                    if (_declaredNames.Contains(name1)) throw new Exception("redeclaration variable");
                    _declaredNames.Add(name1);
                    if (Accept(TokenProgram.Assign))
                    {
                        var expressionNode = Expr();
                        var assignNode = new AssignNode(GetDataNode(name1), expressionNode);
                    }

                    while (Accept(TokenProgram.Comma))
                    {
                        Expect(TokenProgram.Name);
                        var name2 = _names[_namesIdx++];
                        if (_declaredNames.Contains(name2)) throw new Exception("redeclaration variable");
                        _declaredNames.Add(name2);
                        if (Accept(TokenProgram.Assign))
                        {
                            var expressionNode = Expr();
                            var assignNode = new AssignNode(GetDataNode(name2), expressionNode);
                        }
                    }
                }

                Expect(TokenProgram.Sem);
            }

            Body();

            Expect(TokenProgram.Return);
            Expect(TokenProgram.Name);
            var name3 = _names[_namesIdx++];
            var returnNode = new ReturnNode(GetDataNode(name3));
            Expect(TokenProgram.Sem);
        }
        
        public void Parse(in TokenProgram[] tokens, in string[] names, in string[] expressions)
        {
            _declaredNames.Clear();
            
            _tokens = tokens;
            _tokenIdx = 0;

            _names = names;
            _namesIdx = 0;

            _expressions = expressions;
            _expressionIdx = 0;
            
            Program();
        }
    }
}
