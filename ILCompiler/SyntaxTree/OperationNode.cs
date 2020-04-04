using System;
using System.Reflection.Emit;

namespace ILCompiler
{
    public enum Operation { Add, Sub, Mul, Div }
    
    public class OperationNode : ExpressionNode
    {
        private Operation _operation;
        private ExpressionNode[] _arguments;
        
        public OperationNode(Operation operation, ExpressionNode[] arguments)
        {
            _operation = operation;
            _arguments = arguments;
        }

        public override void Generate(Type scope, ILGenerator generator)
        {
            foreach (var argument in _arguments)
            {
                argument.Generate(scope, generator);
            }
            switch (_operation)
            {
                case Operation.Add:
                    generator.Emit(OpCodes.Add);
                    break;
                case Operation.Sub:
                    generator.Emit(OpCodes.Sub);
                    break;
                case Operation.Mul:
                    generator.Emit(OpCodes.Mul);
                    break;
                case Operation.Div:
                    generator.Emit(OpCodes.Div);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}