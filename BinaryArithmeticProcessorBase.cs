using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    public abstract class BinaryArithmeticProcessorBase : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value2 = compiler.CurrentBasicBlock.GetState().StackPop();
            var value1 = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVMValueRef result;
            if(value1.TypeInfo == TypeInfo.FloatingPrimitive)
                result = ProcessFloating(compiler, value1.Value, value2.Value, builder);
            else
                result = ProcessIntegral(compiler, value1.Value, value2.Value, builder);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, value1.TypeInfo));
        }

        protected abstract LLVMValueRef ProcessIntegral(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder);

        protected abstract LLVMValueRef ProcessFloating(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder);
    }
}
