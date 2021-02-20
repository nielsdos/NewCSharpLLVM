using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldc_R4)]
    public class LdcR4Processor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = LLVM.ConstReal(LLVM.FloatType(), (double)(float)insn.Operand);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, TypeInfo.FloatingPrimitive));
        }
    }
}
