using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Div_Un)]
    public class DivUnProcessorBase : BinaryArithmeticProcessorBase
    {
        protected override LLVMValueRef ProcessIntegral(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildUDiv(builder, lhs, rhs, string.Empty);

        protected override LLVMValueRef ProcessFloating(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildFDiv(builder, lhs, rhs, string.Empty);
    }
}
