using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldc_R8)]
    public class LdcR8Processor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = LLVM.ConstReal(LLVM.DoubleType(), (double)(float)insn.Operand);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, TypeInfo.FloatingPrimitive));
        }
    }
}
