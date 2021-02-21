using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Stelem_I4)] // TODO
    public class StElemProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            var index = compiler.CurrentBasicBlock.GetState().StackPop();
            var array = compiler.CurrentBasicBlock.GetState().StackPop();

            // TODO: exceptions?
            var gep = LLVM.BuildInBoundsGEP(builder, array.Value, new LLVMValueRef[] { index.Value }, string.Empty);
            LLVM.BuildStore(builder, value.Value, gep);
        }
    }
}
