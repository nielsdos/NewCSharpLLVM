using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Ldind_U4 // TODO
    )]
    public class LdIndProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var address = compiler.CurrentBasicBlock.GetState().StackPop();
            var value = LLVM.BuildLoad(builder, address.Value, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, TypeInfo.UnIntPrimitive /*TODO*/));
        }
    }
}
