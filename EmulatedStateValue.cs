using LLVMSharp;

namespace CSharpLLVM
{
    public class EmulatedStateValue
    {
        // TODO: type
        public LLVMValueRef Value { get; private set; }
        public BasicBlock Origin { get; private set; } // TODO: can be removed?

        public EmulatedStateValue(LLVMValueRef valueRef, BasicBlock origin)
        {
            Value = valueRef;
            Origin = origin;
        }

        public EmulatedStateValue(LLVMBuilderRef builder, BasicBlock origin, EmulatedStateValue otherValue)
        {
            Origin = origin;

            LLVMValueRef value = otherValue.Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(value), string.Empty);
            LLVMValueRef[] incoming = { value };
            LLVMBasicBlockRef[] basicBlocks = { origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock mergingBasicBlock, EmulatedStateValue other)
        {
            LLVMValueRef[] incoming = { other.Value };
            LLVMBasicBlockRef[] basicBlocks = { /*other.Origin.LLVMBlock*/mergingBasicBlock.LLVMBlock }; // TODO: ??
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }
    }
}
