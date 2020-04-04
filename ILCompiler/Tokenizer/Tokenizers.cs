namespace ILCompiler.Tokenizer
{
    public class Tokenizers
    {
        public static readonly ProgramTokenizer Program = new ProgramTokenizer();
        public static readonly ExpressionTokenizer Expression = new ExpressionTokenizer();
    }
}