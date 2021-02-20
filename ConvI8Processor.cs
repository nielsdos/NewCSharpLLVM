using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Conv_I8)]
    public class ConvI8Processor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            var result = LLVM.BuildIntCast(builder, value.Value, LLVM.Int64Type(), string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, TypeInfo.IntegralPrimitive));
        }
    }
}
