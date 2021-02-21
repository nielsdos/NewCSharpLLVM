using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldfld)]
    public class LdFldProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var obj = compiler.CurrentBasicBlock.GetState().StackPop();
            FieldReference fieldRef = (FieldReference) insn.Operand;
            var value = obj.Value;

            uint idx = compiler.TypeLookup.GetIndexInStructure(fieldRef);

            LLVMValueRef result;
            if(obj.TypeInfo == TypeInfo.Structure)
            {
                // This is a value type, directly go with extracting the value.
                result = LLVM.BuildExtractValue(builder, value, idx, string.Empty);
            }
            else
            {
                // Have to go through a pointer.
                var gep = LLVM.BuildStructGEP(builder, value, (uint)idx, string.Empty);
                result = LLVM.BuildLoad(builder, gep, string.Empty);
            }

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, fieldRef.FieldType, builder));
        }
    }
}
