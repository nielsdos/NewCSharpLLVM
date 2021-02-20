using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;

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

            Debug.Assert(compiler.CurrentBasicBlock.GetState().StackSize == 0);
        }
    }
}
