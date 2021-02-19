using LLVMSharp;

namespace CSharpLLVM
{
    public class EmulatedStateValue
    {
        // TODO: type
        public LLVMValueRef Value { get; private set; }

        public EmulatedStateValue(LLVMValueRef valueRef)
        {
            Value = valueRef;
        }

        public EmulatedStateValue(LLVMBuilderRef builder, BasicBlock origin, EmulatedStateValue otherValue)
        {
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
