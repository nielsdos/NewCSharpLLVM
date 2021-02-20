using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Mul)]
    public class MulProcessorBase : BinaryArithmeticProcessorBase
    {
        protected override LLVMValueRef ProcessIntegral(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildMul(builder, lhs, rhs, string.Empty);

        protected override LLVMValueRef ProcessFloating(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildFMul(builder, lhs, rhs, string.Empty);
    }
}
