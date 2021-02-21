using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Stloc, Code.Stloc_S, Code.Stloc_0, Code.Stloc_1, Code.Stloc_2, Code.Stloc_3)]
    public class StLocProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            Code code = insn.OpCode.Code;

            int index;
            if (code >= Code.Stloc_0 && code <= Code.Stloc_3)
            {
                index = insn.OpCode.Code - Code.Stloc_0;
            }
            else
            {
                VariableDefinition def = (VariableDefinition) insn.Operand;
                index = def.Index;
            }

            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVM.BuildStore(builder, value.Value, compiler.LocalValues[index].Value);
        }
    }
}
