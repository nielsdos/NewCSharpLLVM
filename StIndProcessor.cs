using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Stind_I,
        Code.Stind_I1,
        Code.Stind_I2,
        Code.Stind_I4,
        Code.Stind_I8,
        Code.Stind_R4,
        Code.Stind_R8,
        Code.Stind_Ref
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
