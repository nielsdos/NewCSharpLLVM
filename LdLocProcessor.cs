using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldloc, Code.Ldloc_S, Code.Ldloc_0, Code.Ldloc_1, Code.Ldloc_2, Code.Ldloc_3)]
    public class LdLocProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Code code = insn.OpCode.Code;

            int index;
            if (code >= Code.Ldloc_0 && code <= Code.Ldloc_3)
            {
                index = insn.OpCode.Code - Code.Ldloc_0;
            }
            else
            {
                VariableDefinition def = (VariableDefinition) insn.Operand;
                index = def.Index;
            }

            compiler.CurrentBasicBlock.GetState().StackPush(compiler.CurrentBasicBlock.GetState().Locals[index]);
            compiler.CurrentBasicBlock.GetState().Locals[index] = null;
        }
    }
}
