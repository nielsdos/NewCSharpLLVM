using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Stsfld)]
    public class StsFldProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            FieldReference fieldRef = (FieldReference) insn.Operand;
            var ptr = compiler.StaticFieldLookup.GetField(fieldRef);
            LLVM.BuildStore(builder, value.Value, ptr);
        }
    }
}
