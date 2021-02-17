using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Add)]
    public class AddProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            // TODO: correctly handle overflow
            // TODO: correctly handle typings (see conversion table)

            var value2 = compiler.CurrentBasicBlock.GetState().StackPop();
            var value1 = compiler.CurrentBasicBlock.GetState().StackPop();
            var result = LLVM.BuildAdd(builder, value2.Value, value1.Value, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, compiler.CurrentBasicBlock.LLVMBlock));
        }
    }
}
