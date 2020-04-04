using System;
using System.Collections.Generic;
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
        private int namesIdx = 0;
        
        private string[] _expressions;
        private int expressionIdx = 0;

        private List<string> declaredNames = new List<string>();

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
                Tokenizers.Expression.Tokenize(_expressions[expressionIdx++], out var exprNames,
                    out var exprConstants);

            return Parsers.Expression.Parse(exprTokens, exprNames, exprConstants, declaredNames);
        }

        private void Body()
        {
            while (true)
            {
                if (Accept(TokenProgram.Name))
                {
                    var name = _names[namesIdx++];
                    Expect(TokenProgram.Assign);
                    var expressionNode = Expr();
                    Expect(TokenProgram.Sem);
                } else if (Accept(TokenProgram.If))
                {
                    Expect(TokenProgram.ROpen);
                    var expressionNode = Expr();
                    Expect(TokenProgram.RClose);
                    Expect(TokenProgram.COpen);
                    Body();
                    Expect(TokenProgram.CClose);
                    if (Accept(TokenProgram.Else))
                    {
                        Expect(TokenProgram.COpen);
                        Body();
                        Expect(TokenProgram.CClose);
                    }
                } else if (Accept(TokenProgram.While))
                {
                    Expect(TokenProgram.ROpen);
                    var expressionNode = Expr();
                    Expect(TokenProgram.RClose);
                    Expect(TokenProgram.COpen);
                    Body();
                    Expect(TokenProgram.CClose);
                }
                else
                {
                    break;
                }
            }
        }

        private void Program()
        {
            if (Accept(TokenProgram.Decl))
            {
                if (Accept(TokenProgram.Name))
                {
                    var name1 = _names[namesIdx++];
                    if (Accept(TokenProgram.Assign))
                    {
                        var expressionNode = Expr();
                    }

                    while (Accept(TokenProgram.Comma))
                    {
                        Expect(TokenProgram.Name);
                        var name2 = _names[namesIdx++];
                        if (Accept(TokenProgram.Assign))
                        {
                            var expressionNode = Expr();
                        }
                    }
                }

                Expect(TokenProgram.Sem);
            }

            Body();

            Expect(TokenProgram.Return);
            Expect(TokenProgram.Name);
            var name3 = _names[namesIdx++];
            Expect(TokenProgram.Sem);
        }
        
        public void Parse(in TokenProgram[] tokens, in string[] names, in string[] expressions)
        {
            _tokens = tokens;
            _tokenIdx = 0;

            _names = names;
            namesIdx = 0;

            _expressions = expressions;
            expressionIdx = 0;
            
            Program();
        }
        
        
    }
}
