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

        public EmulatedStateValue(LLVMBuilderRef builder, BasicBlock origin, EmulatedStateValue otherValue)
        {
            Origin = origin;
            isPhi = true;

            LLVMValueRef old = otherValue.Value;
            Value = LLVM.BuildPhi(builder, LLVM.TypeOf(old), string.Empty);
            LLVMValueRef[] incoming = { old };
            LLVMBasicBlockRef[] basicBlocks = { origin.LLVMBlock };
            LLVM.AddIncoming(Value, incoming, basicBlocks, 1);
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock mergingBasicBlock, EmulatedStateValue other)
        {
            Console.WriteLine("      Construct phi node! " + isPhi);

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
            //Origin = mergingBasicBlock;
        }
    }
}
