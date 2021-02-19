using LLVMSharp;
using Mono.Cecil;

namespace CSharpLLVM
{
    public enum TypeInfo
    {
        ValueType,
        Primitive,
        Reference,
    }

    public class EmulatedStateValue
    {
        public LLVMValueRef Value { get; private set; }
        public TypeInfo TypeInfo { get; private set; }

        public EmulatedStateValue(LLVMValueRef valueRef, TypeInfo typeInfo)
        {
            Value = valueRef;
            TypeInfo = typeInfo;
        }

        public EmulatedStateValue(LLVMValueRef valueRef, TypeReference typeRef)
             : this(valueRef, typeRef.GetTypeInfo())
        {
        }

        public EmulatedStateValue(LLVMBuilderRef builder, BasicBlock origin, EmulatedStateValue otherValue)
        {
            TypeInfo = otherValue.TypeInfo;
            LLVMValueRef value = otherValue.Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(value), string.Empty);
            LLVMValueRef[] incoming = { value };
            LLVMBasicBlockRef[] basicBlocks = { origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock origin, EmulatedStateValue other)
        {
            LLVMValueRef[] incoming = { other.Value };
            LLVMBasicBlockRef[] basicBlocks = { origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }
    }
}
