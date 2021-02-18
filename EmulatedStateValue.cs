using LLVMSharp;
using System;

namespace CSharpLLVM
{
    public class EmulatedStateValue
    {
        // TODO: type
        public LLVMValueRef Value { get; private set; }
        public BasicBlock Origin { get; private set; }

        private bool isPhi = false;

        public EmulatedStateValue(LLVMValueRef valueRef, BasicBlock origin)
        {
            Value = valueRef;
            Origin = origin;
        }

        public EmulatedStateValue(LLVMBuilderRef builder, EmulatedStateValue otherValue)
        {
            Value = otherValue.Value;
            Origin = otherValue.Origin;
            isPhi = true;

            LLVMValueRef old = Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(Value), string.Empty);
            LLVMValueRef[] incoming = { old };
            LLVMBasicBlockRef[] basicBlocks = { Origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock mergingBasicBlock, EmulatedStateValue other)
        {
            Console.WriteLine("      Construct phi node!");

            if(isPhi)
            {
                LLVMValueRef[] incoming = { other.Value };
                LLVMBasicBlockRef[] basicBlocks = { other.Origin.LLVMBlock };
                LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
            }
            else
            {
                LLVMValueRef old = Value;
                Value = LLVM.BuildPhi(builder, LLVM.TypeOf(Value), string.Empty);
                LLVMValueRef[] incoming = { old, other.Value };
                LLVMBasicBlockRef[] basicBlocks = { Origin.LLVMBlock, other.Origin.LLVMBlock };
                LLVM.AddIncoming(Value, incoming, basicBlocks, 2);
            }

            

            // Phi node predecessor relationship isn't transitive.
            Origin = mergingBasicBlock;
        }
    }
}
