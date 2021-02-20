using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Neg)]
    public class NegProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVMValueRef negated;
            if(value.TypeInfo == TypeInfo.FloatingPrimitive)
                negated = LLVM.BuildFNeg(builder, value.Value, string.Empty);
            else
                negated = LLVM.BuildNeg(builder, value.Value, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(negated, value.TypeInfo));
        }
    }
}
