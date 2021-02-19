using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Nop, Code.Tail)]
    public class NopProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            // Intentionally empty:
            //   - nop compiles to nothing.
            //   - tail is handled in the call processors.
        }
    }
}
