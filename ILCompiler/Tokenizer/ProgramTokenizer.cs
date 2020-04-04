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
            foreach (var rawToken in rawTokens)
            {
                if (TryParse(rawToken, out TokenProgram token))
                {
                    if (!parsingExpression)
                    {
                        tokens.Add(token);
                        if (token == TokenProgram.Assign)
                        {
                            parsingExpression = true;
                            listExpressions.Add(string.Empty);
                            tokens.Add(TokenProgram.Expr);
                        }
                        else if (token == TokenProgram.ROpen)
                        {
                            parsingExpression = true;
                            listExpressions.Add(string.Empty);
                            tokens.Add(TokenProgram.Expr);
                        }
                    }
                    else if (token == TokenProgram.Sem || token == TokenProgram.Comma || token == TokenProgram.RClose)
                    {
                        parsingExpression = false;
                        tokens.Add(token);
                    }
                    else
                    {
                        listExpressions[listExpressions.Count - 1] += rawToken;
                    }
                }
                else if (!parsingExpression)
                {
                    tokens.Add(TokenProgram.Name);
                    if (!IsName(rawToken)) throw new Exception("invalid variable name");
                    listNames.Add(rawToken);
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
