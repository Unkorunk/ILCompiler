using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class IfNode : Node
    {
        private readonly ExpressionNode _expressionNode;
        private readonly Node _thenNode;
        private readonly Node _elseNode;

        private Label _elseLabel, _endLabel;

        public IfNode(ExpressionNode expressionNode, Node thenNode, Node elseNode)
        {
            _expressionNode = expressionNode;
            _thenNode = thenNode;
            _elseNode = elseNode;
        }
        
        public override void Generate(Type scope, ILGenerator generator)
        {
            _elseLabel = generator.DefineLabel();
            _endLabel = generator.DefineLabel();

            _expressionNode.Generate(scope, generator);
            generator.Emit(OpCodes.Brfalse, _elseLabel);
            _thenNode?.FinalGenerate(scope, generator);
            generator.Emit(OpCodes.Br, _endLabel);
            generator.MarkLabel(_elseLabel);
            _elseNode?.FinalGenerate(scope, generator);
            generator.MarkLabel(_endLabel);
        }
    }
}