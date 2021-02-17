using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ret)]
    public class RetProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            // TODO: void

            //if(compiler.MethodDef.ReturnType)


            //LLVM.BuildRetVoid(builder);

            LLVM.BuildRet(builder, compiler.CurrentBasicBlock.GetState().StackPop().Value);
        }
    }
}
