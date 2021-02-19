using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ret)]
    public class RetProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            if(compiler.MethodDef.ReturnType.MetadataType == MetadataType.Void)
            {
                LLVM.BuildRetVoid(builder);
            }
            else
            {
                LLVM.BuildRet(builder, compiler.CurrentBasicBlock.GetState().StackPop().Value);
            }
        }
    }
}
