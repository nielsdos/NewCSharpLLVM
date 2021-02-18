using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Brfalse, Code.Brfalse_S)]
    public class BrFalseProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Instruction destination = (Instruction) insn.Operand;
            var elseBlock = compiler.GetBasicBlock(destination.Offset);
            var thenBlock = compiler.GetBasicBlock(insn.Next.Offset);
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVM.BuildCondBr(builder, value.Value, thenBlock.LLVMBlock, elseBlock.LLVMBlock);
        }
    }
}
