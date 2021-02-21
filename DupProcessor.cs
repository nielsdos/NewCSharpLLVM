using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Dup)]
    public class DupProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPeek();
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value));
        }
    }
}
