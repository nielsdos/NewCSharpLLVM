using LLVMSharp;
using Mono.Cecil.Cil;
using System;

namespace CSharpLLVM
{
    [InstructionHandler(Code.And, Code.Or, Code.Xor, Code.Shl, Code.Shr, Code.Shr_Un)]
    public class BinaryLogicProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value2 = compiler.CurrentBasicBlock.GetState().StackPop();
            var value1 = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVMValueRef result;
            Code code = insn.OpCode.Code;
            switch (code)
            {
                case Code.And:
                    result = LLVM.BuildAnd(builder, value1.Value, value2.Value, string.Empty);
                    break;

                case Code.Or:
                    result = LLVM.BuildOr(builder, value1.Value, value2.Value, string.Empty);
                    break;

                case Code.Xor:
                    result = LLVM.BuildXor(builder, value1.Value, value2.Value, string.Empty);
                    break;

                case Code.Shl:
                    result = LLVM.BuildShl(builder, value1.Value, value2.Value, string.Empty);
                    break;

                case Code.Shr:
                    result = LLVM.BuildAShr(builder, value1.Value, value2.Value, string.Empty);
                    break;

                case Code.Shr_Un:
                    result = LLVM.BuildLShr(builder, value1.Value, value2.Value, string.Empty);
                    break;

                default:
                    throw new InvalidOperationException("Unexpected code " + code);
            }
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, value1.TypeInfo));
        }
    }
}
