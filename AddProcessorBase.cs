using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Add)]
    public class AddProcessor : BinaryArithmeticProcessorBase
    {
        protected override LLVMValueRef ProcessIntegral(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildAdd(builder, lhs, rhs, string.Empty);

        protected override LLVMValueRef ProcessFloating(MethodCompiler compiler, LLVMValueRef lhs, LLVMValueRef rhs, LLVMBuilderRef builder)
            => LLVM.BuildFAdd(builder, lhs, rhs, string.Empty);
    }
}
