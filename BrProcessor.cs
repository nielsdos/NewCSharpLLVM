using LLVMSharp;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Br, Code.Br_S)]
    public class BrProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Instruction destination = (Instruction) insn.Operand;
            bool status = compiler.GetBasicBlock(destination.Offset, out var basicBlock);
            Debug.Assert(status);
            LLVM.BuildBr(builder, basicBlock.LLVMBlock);
        }
    }
}
