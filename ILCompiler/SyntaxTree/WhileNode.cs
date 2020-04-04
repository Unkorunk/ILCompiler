using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class WhileNode : Node
    {
        private readonly ExpressionNode _expressionNode;
        private readonly Node _insideNode;
        private Label _insideLabel, _outsideLabel;
        
        public WhileNode(ExpressionNode expressionNode, Node insideNode)
        {
            _expressionNode = expressionNode;
            _insideNode = insideNode;
        }
        
        public override void Generate(Type scope, ILGenerator generator)
        {
            _insideLabel = generator.DefineLabel();
            _outsideLabel = generator.DefineLabel();
            
            generator.MarkLabel(_insideLabel);
            _expressionNode.Generate(scope, generator);
            generator.Emit(OpCodes.Brfalse, _outsideLabel);
            _insideNode?.FinalGenerate(scope, generator);
            generator.Emit(OpCodes.Br, _insideLabel);
            generator.MarkLabel(_outsideLabel);
        }
    }
}