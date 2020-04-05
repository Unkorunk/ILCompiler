using System;
using System.Collections.Generic;
using ILCompiler.Token;

namespace ILCompiler.Tokenizer
{
    public class ProgramTokenizer : Tokenizer<TokenProgram>
    {
        protected override bool TryParse(string rawToken, out TokenProgram tokenProgram)
        {
            switch (rawToken)
            {
                case "if":
                    tokenProgram = TokenProgram.If;
                    return true;
                case "else":
                    tokenProgram = TokenProgram.Else;
                    return true;
                case "return":
                    tokenProgram = TokenProgram.Return;
                    return true;
                case "decl":
                    tokenProgram = TokenProgram.Decl;
                    return true;
                case ";":
                    tokenProgram = TokenProgram.Sem;
                    return true;
                case "=":
                    tokenProgram = TokenProgram.Assign;
                    return true;
                case "(":
                    tokenProgram = TokenProgram.ROpen;
                    return true;
                case ")":
                    tokenProgram = TokenProgram.RClose;
                    return true;
                case "{":
                    tokenProgram = TokenProgram.COpen;
                    return true;
                case "}":
                    tokenProgram = TokenProgram.CClose;
                    return true;
                case ",":
                    tokenProgram = TokenProgram.Comma;
                    return true;
                case "while":
                    tokenProgram = TokenProgram.While;
                    return true;
            }

            tokenProgram = TokenProgram.Assign;
            return false;
        }

        protected override TokenProgram Parse(string rawToken)
        {
            if (TryParse(rawToken, out var tokenProgram))
            {
                return tokenProgram;
            }
            throw new Exception("invalid raw token");
        }

        public TokenProgram[] Tokenize(string sourceText, out string[] names, out string[] expressions)
        {
            var symbols = new[] {"if", "else", "return", "decl", ";", "(", ")", "{", "}", ",", "while"};
            foreach (var symbol in symbols)
            {
                sourceText = sourceText.Replace(symbol, " " + symbol + " ");
            }

            // process "=" (without destroy "!=" and "==") => " = "
            for (var i = 1; i < sourceText.Length - 1; i++)
            {
                if (sourceText[i] == '=' && sourceText[i - 1] != '!' && sourceText[i - 1] != '=' &&
                    sourceText[i + 1] != '=')
                {
                    sourceText = sourceText.Remove(i, 1).Insert(i, " = ");
                    i += 2;
                }
            }

            var rawTokens = sourceText.Split(new[] {" ", Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var tokens = new List<TokenProgram>();
            var listNames = new List<string>();
            var listExpressions = new List<string>();
            
            var parsingExpression = false;
            TokenProgram startToken = TokenProgram.Assign;
            var balance = 0;
            var startBalance = 0;
            
            foreach (var rawToken in rawTokens)
            {
                if (TryParse(rawToken, out TokenProgram token))
                {
                    if (token == TokenProgram.ROpen)
                    {
                        balance++;
                    } else if (token == TokenProgram.RClose)
                    {
                        balance--;
                    }
                    
                    if (!parsingExpression)
                    {
                        tokens.Add(token);
                        if (token == TokenProgram.Assign || token == TokenProgram.ROpen || token == TokenProgram.Return)
                        {
                            parsingExpression = true;
                            listExpressions.Add(string.Empty);
                            tokens.Add(TokenProgram.Expr);
                            if (token == TokenProgram.Assign)
                            {
                                startToken = TokenProgram.Assign;
                            } else if (token == TokenProgram.Return)
                            {
                                startToken = TokenProgram.Return;
                            } else if (token == TokenProgram.ROpen)
                            {
                                startToken = TokenProgram.ROpen;
                            }
                            startBalance = balance;
                        }
                    }
                    else
                    {
                        if (startToken == TokenProgram.Assign && (token == TokenProgram.Comma || token == TokenProgram.Sem) ||
                            startToken == TokenProgram.ROpen && token == TokenProgram.RClose && balance == startBalance - 1 ||
                            startToken == TokenProgram.Return && token == TokenProgram.Sem)
                        {
                            parsingExpression = false;
                            tokens.Add(token);
                        }
                        else
                        {
                            listExpressions[listExpressions.Count - 1] += rawToken;
                        }
                    }
                }
                else if (!parsingExpression)
                {
                    if (IsName(rawToken))
                    {
                        tokens.Add(TokenProgram.Name);
                        listNames.Add(rawToken);
                    }
                    else
                    {
                        throw new Exception("invalid data");
                    }
                }
                else
                {
                    listExpressions[listExpressions.Count - 1] += rawToken;
                }
            }

            names = listNames.ToArray();
            expressions = listExpressions.ToArray();

            return tokens.ToArray();
        }
    }
}
