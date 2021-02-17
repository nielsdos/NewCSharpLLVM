using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    public interface InstructionProcessor
    {
        // TODO: provide shorthand for getting the state

        void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder);
    }
}
