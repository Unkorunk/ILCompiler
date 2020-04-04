using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class ReturnNode : Node
    {
        private DataNode _dataNode;
        
        public ReturnNode(DataNode dataNode)
        {
            _dataNode = dataNode;
        }
        
        public override void Generate(Type scope, ILGenerator generator)
        {
            _dataNode.Generate(scope, generator);
            generator.Emit(OpCodes.Ret);
        }
    }
}