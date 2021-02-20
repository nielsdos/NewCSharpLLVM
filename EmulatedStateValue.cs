using LLVMSharp;
using Mono.Cecil;

namespace CSharpLLVM
{
    public enum TypeInfo
    {
        ValueType,
        IntegralPrimitive,
        FloatingPrimitive,
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

        public EmulatedStateValue(LLVMValueRef valueRef, TypeReference typeRef, LLVMBuilderRef builder)
        {
            // Stack conversions as required by the CIL spec.
            var mdt = typeRef.MetadataType;
            if(mdt == MetadataType.Byte || mdt == MetadataType.UInt16 || mdt == MetadataType.Boolean || mdt == MetadataType.Char)
            {
                valueRef = LLVM.BuildZExt(builder, valueRef, LLVM.Int32Type(), string.Empty);
            }
            else if(mdt == MetadataType.SByte || mdt == MetadataType.Int16)
            {
                valueRef = LLVM.BuildSExt(builder, valueRef, LLVM.Int32Type(), string.Empty);
            }

            Value = valueRef;
            TypeInfo = typeRef.GetTypeInfo();
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
