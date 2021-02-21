using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Ldelem_I4)] // TODO
    public class LdElemProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var index = compiler.CurrentBasicBlock.GetState().StackPop();
            var array = compiler.CurrentBasicBlock.GetState().StackPop();

            // TODO: exceptions?
            var gep = LLVM.BuildInBoundsGEP(builder, array.Value, new LLVMValueRef[] { index.Value }, string.Empty);
            var value = LLVM.BuildLoad(builder, gep, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, TypeInfo.SiIntPrimitive/*TODO*/));
        }
    }
}
