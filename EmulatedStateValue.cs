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

        public EmulatedStateValue(EmulatedStateValue otherValue)
        {
            Value = otherValue.Value;
            Origin = otherValue.Origin;
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock mergingBasicBlock, EmulatedStateValue other)
        {
            Console.WriteLine("      Construct phi node!");

            // If the inner value references the same value,
            // it means this value is independent of the other state.
            LLVMValueRef old = Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(Value), string.Empty);
            LLVMValueRef[] incoming = { old, other.Value };
            LLVMBasicBlockRef[] basicBlocks = { Origin.LLVMBlock, other.Origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 2);

            // Phi node predecessor relationship isn't transitive.
            Origin = mergingBasicBlock;
        }
    }
}
