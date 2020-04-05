using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public enum Operation { Add, Sub, Mul, Div, Great, Less, Equal, UnEqual, LogicalOr, LogicalAnd, LogicalNot, Function }
    
    public class OperationNode : ExpressionNode
    {
        private Operation _operation;
        private ExpressionNode[] _arguments;
        
        private string _functionName;
        private Type _returnType;
        
        public OperationNode(Operation operation, ExpressionNode[] arguments)
        {
            _operation = operation;
            _arguments = arguments;
            _functionName = string.Empty;
            _returnType = null;
        }
        
        public OperationNode(Operation operation, ExpressionNode[] arguments, 
            string functionName, Type returnType) : this(operation, arguments)
        {
            _functionName = functionName;
            _returnType = returnType;
        }

        public override void Generate(Type scope, ILGenerator generator)
        {
            foreach (var argument in _arguments)
            {
                argument.Generate(scope, generator);
                if (_operation == Operation.LogicalOr || _operation == Operation.LogicalAnd || _operation == Operation.LogicalNot)
                {
                    generator.Emit(OpCodes.Ldc_I8, 0L);
                    generator.Emit(OpCodes.Cgt_Un);
                    generator.Emit(OpCodes.Conv_I8);
                }
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
                    generator.Emit(OpCodes.Ceq);
                    generator.Emit(OpCodes.Conv_I8);
                    generator.Emit(OpCodes.Ldc_I8, 1L);
                    generator.Emit(OpCodes.Xor);
                    break;
                case Operation.LogicalAnd:
                    generator.Emit(OpCodes.And);
                    break;
                case Operation.LogicalOr:
                    generator.Emit(OpCodes.Or);
                    break;
                case Operation.LogicalNot:
                    generator.Emit(OpCodes.Ldc_I8, 1L);
                    generator.Emit(OpCodes.Xor);
                    break;
                case Operation.Function:
                    var methods = scope.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                                  BindingFlags.Public | BindingFlags.Static);
                    MethodInfo methodInfo = null;
                    foreach (var method in methods)
                    {
                        if (method.Name == _functionName && method.ReturnType == _returnType &&
                            method.GetParameters().Length == _arguments.Length)
                        {
                            var valid = true;
                            foreach (var parameter in method.GetParameters())
                            {
                                if (parameter.ParameterType != typeof(long))
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (valid)
                            {
                                methodInfo = method;
                            }
                        }
                    }

                    if (methodInfo == null) throw new Exception("invalid function");
                    
                    generator.Emit(OpCodes.Call, methodInfo);
                    
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}