using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    public interface InstructionProcessor
    {
        void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder);
    }
}
