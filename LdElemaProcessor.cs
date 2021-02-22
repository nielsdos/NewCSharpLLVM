using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldelema)]
    public class LdElemaProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var index = compiler.CurrentBasicBlock.GetState().StackPop();
            var array = compiler.CurrentBasicBlock.GetState().StackPop();

            // TODO: exceptions?
            var gep = LLVM.BuildInBoundsGEP(builder, array.Value, new LLVMValueRef[] { index.Value }, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(gep, TypeInfo.Reference));
        }
    }
}
