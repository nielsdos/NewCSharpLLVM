using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Rem)]
    public class RemProcessor : BinaryArithmeticProcessorBase
    {
        protected override LLVMValueRef ProcessIntegral(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildSRem(builder, lhs, rhs, string.Empty);

        protected override LLVMValueRef ProcessFloating(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildFRem(builder, lhs, rhs, string.Empty);
    }
}
