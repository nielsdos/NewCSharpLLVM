using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Br, Code.Br_S)]
    public class BrProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Instruction destination = (Instruction) insn.Operand;
            var basicBlock = compiler.GetBasicBlock(destination.Offset);
            LLVM.BuildBr(builder, basicBlock.LLVMBlock);
        }
    }
}
