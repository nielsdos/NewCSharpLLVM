using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Add, Code.Sub, Code.Mul, Code.Div, Code.Div_Un)]
    public class BinaryArithmeticProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            // TODO: correctly handle overflow
            // TODO: correctly handle typings (see conversion table)

            var value2 = compiler.CurrentBasicBlock.GetState().StackPop();
            var value1 = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVMValueRef result;
            if(insn.OpCode.Code == Code.Add)
                result = LLVM.BuildAdd(builder, value1.Value, value2.Value, string.Empty);
            else if(insn.OpCode.Code == Code.Mul)
                result = LLVM.BuildMul(builder, value1.Value, value2.Value, string.Empty);
            else if(insn.OpCode.Code == Code.Div)
                result = LLVM.BuildSDiv(builder, value1.Value, value2.Value, string.Empty);
            else if(insn.OpCode.Code == Code.Div_Un)
                result = LLVM.BuildUDiv(builder, value1.Value, value2.Value, string.Empty);
            else
                result = LLVM.BuildSub(builder, value1.Value, value2.Value, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, TypeInfo.Primitive));
        }
    }
}
