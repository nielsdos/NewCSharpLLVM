using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldloca, Code.Ldloca_S)]
    public class LdLocaProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            VariableDefinition def = (VariableDefinition) insn.Operand;
            int index = def.Index;

            var local = compiler.LocalValues[index];
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(local.Value, TypeInfo.Reference));
        }
    }
}
