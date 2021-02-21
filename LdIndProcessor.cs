using LLVMSharp;
using Mono.Cecil.Cil;
using System;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Ldind_I,
        Code.Ldind_I1,
        Code.Ldind_I2,
        Code.Ldind_I4,
        Code.Ldind_I8,
        Code.Ldind_U1,
        Code.Ldind_U2,
        Code.Ldind_U4,
        Code.Ldind_R4,
        Code.Ldind_R8,
        Code.Ldind_Ref
    )]
    public class LdIndProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var address = compiler.CurrentBasicBlock.GetState().StackPop();
            var value = LLVM.BuildLoad(builder, address.Value, string.Empty);

            TypeInfo typeInfo;

            Code code = insn.OpCode.Code;
            switch(code)
            {
                case Code.Ldind_I:
                case Code.Ldind_I1:
                case Code.Ldind_I2:
                case Code.Ldind_I4:
                case Code.Ldind_I8:
                    typeInfo = TypeInfo.SiIntPrimitive;
                    break;

                case Code.Ldind_U1:
                case Code.Ldind_U2:
                case Code.Ldind_U4:
                    typeInfo = TypeInfo.UnIntPrimitive;
                    break;

                case Code.Ldind_R4:
                case Code.Ldind_R8:
                    typeInfo = TypeInfo.FloatingPrimitive;
                    break;

                case Code.Ldind_Ref:
                    typeInfo = TypeInfo.Reference;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected code " + code);
            }

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, typeInfo));
        }
    }
}
