using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public abstract class Node
    {
        public Node NextNode = null;

        public abstract void Generate(Type scope, ILGenerator generator);
    }
}