using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Ldc_I4_0,
        Code.Ldc_I4_1,
        Code.Ldc_I4_2,
        Code.Ldc_I4_3,
        Code.Ldc_I4_4,
        Code.Ldc_I4_5,
        Code.Ldc_I4_6,
        Code.Ldc_I4_7,
        Code.Ldc_I4_8,
        Code.Ldc_I4_M1,
        Code.Ldc_I4_S,
        Code.Ldc_I4
    )]
    public class LdcI4Processor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            LLVMValueRef value;

            Code code = insn.OpCode.Code;
            if (code >= Code.Ldc_I4_0 && code <= Code.Ldc_I4_8)
            {
                value = LLVM.ConstInt(LLVM.Int32Type(), (ulong)(insn.OpCode.Code - Code.Ldc_I4_0), true);
            }
            else if (code == Code.Ldc_I4_M1)
            {
                unchecked
                {
                    value = LLVM.ConstInt(LLVM.Int32Type(), (uint)-1, true);
                }
            }
            else
            {
                if (insn.Operand is sbyte)
                {
                    value = LLVM.ConstInt(LLVM.Int32Type(), (ulong)(sbyte)insn.Operand, true);
                }
                else
                {
                    unchecked
                    {
                        value = LLVM.ConstInt(LLVM.Int32Type(), (uint)(int)insn.Operand, true);
                    }
                }
            }

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, TypeInfo.SiIntPrimitive));
        }
    }
}
