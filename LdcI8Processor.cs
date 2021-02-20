using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldc_I8)]
    public class LdcI8Processor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            LLVMValueRef value;
            unchecked
            {
                value = LLVM.ConstInt(LLVM.Int64Type(), (ulong)(long)insn.Operand, false);
            }
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, TypeInfo.IntegralPrimitive));
        }
    }
}
