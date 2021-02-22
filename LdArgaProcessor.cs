using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldarga, Code.Ldarga_S)]
    public class LdArgaProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            VariableDefinition def = (VariableDefinition) insn.Operand;
            int index = def.Index;

            var arg = compiler.ArgumentValues[index];
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(arg.Value, TypeInfo.Reference));
        }
    }
}
