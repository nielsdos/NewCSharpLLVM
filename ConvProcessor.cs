using LLVMSharp;
using Mono.Cecil.Cil;
using System;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Conv_R4, Code.Conv_I8)]
    public class ConvProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value = compiler.CurrentBasicBlock.GetState().StackPop();
            LLVMValueRef result;
            TypeInfo typeInfo;

            Code code = insn.OpCode.Code;
            switch(code)
            {
                case Code.Conv_R4:
                    // TODO: unsigned?
                    result = LLVM.BuildSIToFP(builder, value.Value, LLVM.FloatType(), string.Empty);
                    typeInfo = TypeInfo.FloatingPrimitive;
                    break;

                case Code.Conv_I8:
                    // TODO: unsigned?
                    if(value.TypeInfo == TypeInfo.FloatingPrimitive)
                        result = LLVM.BuildFPToSI(builder, value.Value, LLVM.Int64Type(), string.Empty);
                    else
                        result = LLVM.BuildIntCast(builder, value.Value, LLVM.Int64Type(), string.Empty);
                    typeInfo = TypeInfo.IntegralPrimitive;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected code " + code);
            }

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, typeInfo));
        }
    }
}
