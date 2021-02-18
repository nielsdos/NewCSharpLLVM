using LLVMSharp;
using System;

namespace CSharpLLVM
{
    public class EmulatedStateValue
    {
        // TODO: type
        public LLVMValueRef Value { get; private set; }
        public BasicBlock Origin { get; private set; }

        public EmulatedStateValue(LLVMValueRef valueRef, BasicBlock origin)
        {
            Value = valueRef;
            Origin = origin;
        }

        public EmulatedStateValue(LLVMBuilderRef builder, BasicBlock origin, EmulatedStateValue otherValue)
        {
            Origin = origin;

            LLVMValueRef old = otherValue.Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(old), string.Empty);
            LLVMValueRef[] incoming = { old };
            LLVMBasicBlockRef[] basicBlocks = { origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock mergingBasicBlock, EmulatedStateValue other)
        {
            LLVMValueRef[] incoming = { other.Value };
            LLVMBasicBlockRef[] basicBlocks = { other.Origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }
    }
}
