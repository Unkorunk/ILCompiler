namespace ILCompiler.Parser
{
    public class Parsers
    {
        public static readonly ProgramParser Program = new ProgramParser();
        public static readonly ExpressionParser Expression = new ExpressionParser();
    }
}