using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldsfld)]
    public class LdsFldProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            FieldReference fieldRef = (FieldReference) insn.Operand;
            var ptr = compiler.StaticFieldLookup.GetField(fieldRef);
            var value = LLVM.BuildLoad(builder, ptr, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, fieldRef.FieldType.GetTypeInfo()));
        }
    }
}
