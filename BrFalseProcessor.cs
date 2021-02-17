using LLVMSharp;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Brfalse, Code.Brfalse_S)]
    public class BrFalseProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Instruction destination = (Instruction) insn.Operand;
            bool status = compiler.GetBasicBlock(destination.Offset, out var elseBlock);
            Debug.Assert(status);
            status = compiler.GetBasicBlock(insn.Next.Offset, out var thenBlock);
            Debug.Assert(status);
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVM.BuildCondBr(builder, value.Value, thenBlock.LLVMBlock, elseBlock.LLVMBlock);
        }
    }
}
