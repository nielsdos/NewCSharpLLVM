using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Brfalse, Code.Brfalse_S, Code.Brtrue, Code.Brtrue_S)]
    public class BrTrueFalseProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Instruction destination = (Instruction) insn.Operand;
            var b1 = compiler.GetBasicBlock(destination.Offset);
            var b2 = compiler.GetBasicBlock(insn.Next.Offset);
            var value = compiler.CurrentBasicBlock.GetState().StackPop();

            if(insn.OpCode.Code == Code.Brtrue || insn.OpCode.Code == Code.Brtrue_S)
                LLVM.BuildCondBr(builder, value.Value, b1.LLVMBlock, b2.LLVMBlock);
            else
                LLVM.BuildCondBr(builder, value.Value, b2.LLVMBlock, b1.LLVMBlock);
        }
    }
}
