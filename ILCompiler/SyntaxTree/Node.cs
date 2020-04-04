using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public abstract class Node
    {
        public Node NextNode = null;

        public abstract void Generate(Type scope, ILGenerator generator);

        public void FinalGenerate(Type scope, ILGenerator generator)
        {
            Generate(scope, generator);
            
            var startNode = NextNode;
            while (startNode != null)
            {
                startNode.Generate(scope, generator);
                startNode = startNode.NextNode;
            }
        }
    }
}