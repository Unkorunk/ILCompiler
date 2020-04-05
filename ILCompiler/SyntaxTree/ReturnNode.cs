using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class ReturnNode : Node
    {
        private ExpressionNode _expressionNode;
        private bool _flowReturn;
        
        public ReturnNode(ExpressionNode expressionNode)
        {
            _expressionNode = expressionNode;
            _flowReturn = true;
        }

        public ReturnNode(ExpressionNode expressionNode, bool flowReturn)
        {
            _expressionNode = expressionNode;
            _flowReturn = false;
        }

        public override void Generate(Type scope, ILGenerator generator)
        {
            _expressionNode.Generate(scope, generator);
            generator.Emit(OpCodes.Ret);
            FlowReturn = _flowReturn;
        }
    }
}