using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class ReturnNode : Node
    {
        private DataNode _dataNode;
        private bool _flowReturn;
        
        public ReturnNode(DataNode dataNode)
        {
            _dataNode = dataNode;
            _flowReturn = true;
        }

        public ReturnNode(DataNode dataNode, bool flowReturn)
        {
            _dataNode = dataNode;
            _flowReturn = false;
        }
        
        public override void Generate(Type scope, ILGenerator generator)
        {
            _dataNode.Generate(scope, generator);
            generator.Emit(OpCodes.Ret);
            FlowReturn = _flowReturn;
        }
    }
}