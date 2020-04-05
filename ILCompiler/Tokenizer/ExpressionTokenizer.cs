using System;
using System.Collections.Generic;
using ILCompiler.Token;

namespace ILCompiler.Tokenizer
{
    public class ExpressionTokenizer : Tokenizer<TokenExpression>
    {
        protected override TokenExpression Parse(string rawToken)
        {
            switch (rawToken)
            {
                case "+": return TokenExpression.Add;
                case "-": return TokenExpression.Sub;
                case "*": return TokenExpression.Mul;
                case "/": return TokenExpression.Div;
                case ">": return TokenExpression.Great;
                case "<": return TokenExpression.Less;
                case "==": return TokenExpression.Equal;
                case "!=": return TokenExpression.UnEqual;
                case "&&": return TokenExpression.LogicalAnd;
                case "||": return TokenExpression.LogicalOr;
                case "(": return TokenExpression.Open;
                case ")": return TokenExpression.Close;
                case "<=": return TokenExpression.LessEqual;
                case ">=": return TokenExpression.GreatEqual;
                case "!": return TokenExpression.LogicalNot;
            }

            if (long.TryParse(rawToken, out _))
            {
                return TokenExpression.Const;
            }

            if (!IsName(rawToken)) throw new Exception("invalid variable name");

            return TokenExpression.Name;
        }

        protected override bool TryParse(string rawToken, out TokenExpression token)
        {
            token = Parse(rawToken);
            return true;
        }

        public TokenExpression[] Tokenize(string expressionText, out string[] names, out long[] constants)
        {
            var symbols = new[] {"+", "-", "*", "/", "<=", ">=", "==", "!=", "&&", "||", "(", ")"};
            foreach (var symbol in symbols)
            {
                expressionText = expressionText.Replace(symbol, " " + symbol + " ");
            }
            AddSpace(ref expressionText, '<', null, new []{'='});
            AddSpace(ref expressionText, '>', null, new []{'='});
            AddSpace(ref expressionText, '!', null, new []{'='});

            var rawTokens =
                expressionText.Split(new[] {" ", Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var tokens = new TokenExpression[rawTokens.Length];
            var listNames = new List<string>();
            var listConstants = new List<long>();

            for (int i = 0; i < rawTokens.Length; i++)
            {
                tokens[i] = Parse(rawTokens[i]);
                if (tokens[i] == TokenExpression.Name)
                {
                    listNames.Add(rawTokens[i]);
                } else if (tokens[i] == TokenExpression.Const)
                {
                    listConstants.Add(long.Parse(rawTokens[i]));
                }
            }

            names = listNames.ToArray();
            constants = listConstants.ToArray();

            return tokens;
        }
    }
}
