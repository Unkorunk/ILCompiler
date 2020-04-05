namespace ILCompiler.Tokenizer
{
    public abstract class Tokenizer<T>
    {
        protected static bool IsName(string rawToken)
        {
            if (rawToken.Length != 0 && (char.IsLetter(rawToken[0]) || rawToken[0] == '_'))
            {
                for (var i = 1; i < rawToken.Length; i++)
                {
                    if (!char.IsLetter(rawToken[i]) && !char.IsDigit(rawToken[i]) && rawToken[i] != '_')
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        protected static void AddSpace(ref string sourceText, char symbol, char[] ignoreLeft, char[] ignoreRight)
        {
            for (var i = 0; i < sourceText.Length; i++)
            {
                if (sourceText[i] == symbol)
                {
                    var skip = false;
                    if (i != 0 && ignoreLeft != null)
                    {
                        foreach (var ignoreChar in ignoreLeft)
                        {
                            if (sourceText[i - 1] == ignoreChar)
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (i != sourceText.Length - 1 && ignoreRight != null)
                    {
                        foreach (var ignoreChar in ignoreRight)
                        {
                            if (sourceText[i + 1] == ignoreChar)
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (skip) continue;
                    
                    sourceText = sourceText.Remove(i, 1).Insert(i, " " + symbol + " ");
                    i += 2;
                }
            }
        }

        protected abstract bool TryParse(string rawToken, out T token);
        protected abstract T Parse(string rawToken);
    }
}
