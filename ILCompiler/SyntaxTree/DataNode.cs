using System;
using System.Reflection.Emit;

namespace ILCompiler.SyntaxTree
{
    public class DataNode : ExpressionNode
    {
        private enum DataType
        {
            Const,
            Declared,
            Static
        }

        private DataType _dataType;
        private object _source;

        public DataNode(long val)
        {
            _source = val;
            _dataType = DataType.Const;
        }

        public DataNode(string dataName)
        {
            _source = dataName;
            _dataType = DataType.Static;
        }

        public DataNode(LocalBuilder localBuilder)
        {
            _source = localBuilder;
            _dataType = DataType.Declared;
        }

        public void Store(Type scope, ILGenerator generator)
        {
            switch (_dataType)
            {
                case DataType.Declared:
                    generator.Emit(OpCodes.Stloc, (LocalBuilder)_source);
                    break;
                case DataType.Static:
                    var fieldInfo = scope.GetField((string) _source);
                    if (fieldInfo.FieldType != typeof(long))
                    {
                        throw new Exception("invalid field type");
                    }
                    generator.Emit(OpCodes.Stfld, fieldInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Generate(Type scope, ILGenerator generator)
        {
            switch (_dataType)
            {
                case DataType.Const:
                    generator.Emit(OpCodes.Ldc_I8, (long)_source);
                    break;
                case DataType.Declared:
                    generator.Emit(OpCodes.Ldloc, (LocalBuilder)_source);
                    break;
                case DataType.Static:
                    var fieldInfo = scope.GetField((string) _source);
                    if (fieldInfo.FieldType != typeof(long))
                    {
                        throw new Exception("invalid field type");
                    }
                    generator.Emit(OpCodes.Ldfld, fieldInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}