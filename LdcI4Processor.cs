using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldc_I4_1)]//TODO
    public class LdcI4Processor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            // TODO
            var value = LLVM.ConstInt(LLVM.Int32Type(), 1, new LLVMBool(0));
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, compiler.CurrentBasicBlock.LLVMBlock));
        }
    }
}
