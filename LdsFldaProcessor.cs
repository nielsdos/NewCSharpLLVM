using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldsflda)]
    public class LdsFldaProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            FieldReference fieldRef = (FieldReference) insn.Operand;
            var ptr = compiler.StaticFieldLookup.GetField(fieldRef);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(ptr, TypeInfo.Reference));
        }
    }
}
