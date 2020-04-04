using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class AssignNode : Node
    {
        private DataNode _dataNode;
        private ExpressionNode _expressionNode;
        
        public AssignNode(DataNode dataNode, ExpressionNode expressionNode)
        {
            _dataNode = dataNode;
            _expressionNode = expressionNode;
        }
        
        public override void Generate(Type scope, ILGenerator generator)
        {
            _expressionNode.Generate(scope, generator);
            _dataNode.Store(scope, generator);
        }
    }
}