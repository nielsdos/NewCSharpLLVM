using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldflda)]
    public class LdFldaProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var obj = compiler.CurrentBasicBlock.GetState().StackPop();
            FieldReference fieldRef = (FieldReference) insn.Operand;
            var value = obj.Value;

            uint idx = compiler.TypeLookup.GetIndexInStructure(fieldRef);
            var gep = LLVM.BuildStructGEP(builder, value, (uint)idx, string.Empty);

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(gep, TypeInfo.Reference));
        }
    }
}
