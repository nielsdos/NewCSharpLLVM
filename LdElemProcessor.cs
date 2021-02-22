using LLVMSharp;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(
        Code.Ldelem_I,
        Code.Ldelem_I1,
        Code.Ldelem_I2,
        Code.Ldelem_I4,
        Code.Ldelem_I8,
        Code.Ldelem_R4,
        Code.Ldelem_R8,
        Code.Ldelem_U1,
        Code.Ldelem_U2,
        Code.Ldelem_U4
    )]
    public class LdElemProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var index = compiler.CurrentBasicBlock.GetState().StackPop();
            var array = compiler.CurrentBasicBlock.GetState().StackPop();

            TypeInfo typeInfo;
            Code code = insn.OpCode.Code;
            switch (code)
            {
                case Code.Ldelem_U1:
                case Code.Ldelem_U2:
                case Code.Ldelem_U4:
                    typeInfo = TypeInfo.UnIntPrimitive;
                    break;

                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                    typeInfo = TypeInfo.FloatingPrimitive;
                    break;

                default:
                    typeInfo = TypeInfo.SiIntPrimitive;
                    break;
            }

            // TODO: exceptions?
            var gep = LLVM.BuildInBoundsGEP(builder, array.Value, new LLVMValueRef[] { index.Value }, string.Empty);
            var value = LLVM.BuildLoad(builder, gep, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(value, typeInfo));
        }
    }
}
