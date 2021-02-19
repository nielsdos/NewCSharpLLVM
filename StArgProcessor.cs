using LLVMSharp;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Starg, Code.Starg_S)]
    public class StArgProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            ParameterDefinition def = (ParameterDefinition) insn.Operand;
            int index = def.Index;

            var newValue = compiler.CurrentBasicBlock.GetState().StackPop();
            var valuePtr = compiler.ArgumentValues[index];
            LLVM.BuildStore(builder, newValue.Value, valuePtr);
        }
    }
}
