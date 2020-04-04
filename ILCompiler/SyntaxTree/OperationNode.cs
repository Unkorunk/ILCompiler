using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public enum Operation { Add, Sub, Mul, Div, Great, Less, Equal, UnEqual }
    
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
                case Operation.Great:
                    generator.Emit(OpCodes.Cgt);
                    generator.Emit(OpCodes.Conv_I8);
                    break;
                case Operation.Less:
                    generator.Emit(OpCodes.Clt);
                    generator.Emit(OpCodes.Conv_I8);
                    break;
                case Operation.Equal:
                    generator.Emit(OpCodes.Ceq);
                    generator.Emit(OpCodes.Conv_I8);
                    break;
                case Operation.UnEqual:
                    throw new System.NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}