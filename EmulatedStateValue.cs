using LLVMSharp;

namespace CSharpLLVM
{
    public class EmulatedStateValue
    {
        // TODO: type
        public LLVMValueRef Value { get; set; }
        private LLVMBasicBlockRef Origin { get; set; }

        public EmulatedStateValue(LLVMValueRef valueRef, LLVMBasicBlockRef origin)
        {
            Value = valueRef;
            Origin = origin;
        }

        public EmulatedStateValue(EmulatedStateValue otherValue)
        {
            Value = otherValue.Value;
            Origin = otherValue.Origin;
        }

        public void Merge(LLVMBuilderRef builder, LLVMBasicBlockRef mergingBasicBlock, EmulatedStateValue other)
        {
            // If the inner value references the same value,
            // it means this value is independent of the other state.
            LLVMValueRef old = Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(Value), string.Empty);
            LLVMValueRef[] incoming = { old, other.Value };
            LLVMBasicBlockRef[] basicBlocks = { Origin, other.Origin };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 2);
        }
    }
}
