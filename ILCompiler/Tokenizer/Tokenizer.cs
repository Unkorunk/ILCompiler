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

        protected abstract bool TryParse(string rawToken, out T token);
        protected abstract T Parse(string rawToken);
    }
}
