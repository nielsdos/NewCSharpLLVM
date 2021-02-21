using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Stind_I4 // TODO
    )]
    public class StIndProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            var address = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVM.BuildStore(builder, value.Value, address.Value);
        }
    }
}
