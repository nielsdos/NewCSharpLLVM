using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldarg, Code.Ldarg_S, Code.Ldarg_0, Code.Ldarg_1, Code.Ldarg_2, Code.Ldarg_3)]
    public class LdArgProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Code code = insn.OpCode.Code;

            int index;
            if (code >= Code.Ldarg_0 && code <= Code.Ldarg_3)
            {
                index = insn.OpCode.Code - Code.Ldarg_0;
            }
            else
            {
                VariableDefinition def = (VariableDefinition) insn.Operand;
                index = def.Index;
            }

            var arg = compiler.ArgumentValues[index];
            var value = LLVM.BuildLoad(builder, arg.Value, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, arg.TypeInfo));
        }
    }
}
