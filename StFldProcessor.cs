using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Stfld)]
    public class StFldProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            var obj = compiler.CurrentBasicBlock.GetState().StackPop();
            FieldReference fieldRef = (FieldReference) insn.Operand;

            uint idx = compiler.TypeLookup.GetIndexInStructure(fieldRef);

            LLVMValueRef result;
            if(obj.TypeInfo == TypeInfo.Structure)
            {
                // This is a value type, directly go with extracting the value.
                result = LLVM.BuildInsertValue(builder, obj.Value, value.Value, idx, string.Empty);
            }
            else
            {
                // Have to go through a pointer.
                var gep = LLVM.BuildStructGEP(builder, obj.Value, idx, string.Empty);
                LLVM.BuildStore(builder, value.Value, gep);
            }
        }
    }
}
