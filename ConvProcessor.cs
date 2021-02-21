using LLVMSharp;
using Mono.Cecil.Cil;
using System;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Conv_R4,
        Code.Conv_R8,
        Code.Conv_I,
        Code.Conv_I1,
        Code.Conv_I2,
        Code.Conv_I4,
        Code.Conv_I8,
        Code.Conv_U,
        Code.Conv_U1,
        Code.Conv_U2,
        Code.Conv_U4,
        Code.Conv_U8
    )]
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
                    result = ToFP(value, builder, LLVM.FloatType(), out typeInfo);
                    break;

                case Code.Conv_R8:
                    result = ToFP(value, builder, LLVM.DoubleType(), out typeInfo);
                    break;

                case Code.Conv_I:
                    result = ToSiInt(value, builder, compiler.TypeLookup.NativeInt, out typeInfo);
                    break;

                case Code.Conv_I1:
                    result = ToSiInt(value, builder, LLVM.Int8Type(), out typeInfo);
                    break;

                case Code.Conv_I2:
                    result = ToSiInt(value, builder, LLVM.Int16Type(), out typeInfo);
                    break;

                case Code.Conv_I4:
                    result = ToSiInt(value, builder, LLVM.Int32Type(), out typeInfo);
                    break;

                case Code.Conv_I8:
                    result = ToSiInt(value, builder, LLVM.Int64Type(), out typeInfo);
                    break;

                case Code.Conv_U:
                    result = ToUnInt(value, builder, compiler.TypeLookup.NativeInt, out typeInfo);
                    break;

                case Code.Conv_U1:
                    result = ToUnInt(value, builder, LLVM.Int8Type(), out typeInfo);
                    break;

                case Code.Conv_U2:
                    result = ToUnInt(value, builder, LLVM.Int16Type(), out typeInfo);
                    break;

                case Code.Conv_U4:
                    result = ToUnInt(value, builder, LLVM.Int32Type(), out typeInfo);
                    break;

                case Code.Conv_U8:
                    result = ToUnInt(value, builder, LLVM.Int64Type(), out typeInfo);
                    break;

                default:
                    throw new InvalidOperationException("Unexpected code " + code);
            }

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, typeInfo));
        }

        private LLVMValueRef ToSiInt(EmulatedStateValue value, LLVMBuilderRef builder, LLVMTypeRef newType, out TypeInfo typeInfo)
        {
            LLVMValueRef result;
            if(value.TypeInfo == TypeInfo.FloatingPrimitive)
                result = LLVM.BuildFPToSI(builder, value.Value, newType, string.Empty);
            else
                result = LLVM.BuildIntCast(builder, value.Value, newType, string.Empty);
            typeInfo = TypeInfo.SiIntPrimitive;
            return result;
        }

        private LLVMValueRef ToUnInt(EmulatedStateValue value, LLVMBuilderRef builder, LLVMTypeRef newType, out TypeInfo typeInfo)
        {
            LLVMValueRef result;
            if(value.TypeInfo == TypeInfo.FloatingPrimitive)
                result = LLVM.BuildFPToUI(builder, value.Value, newType, string.Empty);
            else
                result = LLVM.BuildIntCast(builder, value.Value, newType, string.Empty);
            typeInfo = TypeInfo.SiIntPrimitive;
            return result;
        }

        private LLVMValueRef ToFP(EmulatedStateValue value, LLVMBuilderRef builder, LLVMTypeRef newType, out TypeInfo typeInfo)
        {
            LLVMValueRef result;
            if(value.TypeInfo == TypeInfo.SiIntPrimitive)
                result = LLVM.BuildSIToFP(builder, value.Value, newType, string.Empty);
            else if(value.TypeInfo == TypeInfo.UnIntPrimitive)
                result = LLVM.BuildUIToFP(builder, value.Value, newType, string.Empty);
            else
                result = LLVM.BuildFPCast(builder, value.Value, newType, string.Empty);
            typeInfo = TypeInfo.FloatingPrimitive;
            return result;
        }
    }
}
