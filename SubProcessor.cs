using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Sub)]
    public class SubProcessorBase : BinaryArithmeticProcessorBase
    {
        protected override LLVMValueRef ProcessIntegral(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildSub(builder, lhs, rhs, string.Empty);

        protected override LLVMValueRef ProcessFloating(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildFSub(builder, lhs, rhs, string.Empty);
    }
}
