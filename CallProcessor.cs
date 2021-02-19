using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Call)]
    public class CallProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            MethodReference methodRef = (MethodReference) insn.Operand;

            // TODO
            if(methodRef.FullName == "System.Void System.Object::.ctor()")return;

            int argCount = methodRef.Parameters.Count;
            LLVMValueRef[] args = new LLVMValueRef[argCount];
            for(int i = 0; i < argCount; ++i)
            {
                args[argCount - i - 1] = compiler.CurrentBasicBlock.GetState().StackPop().Value;
            }

            var ret = LLVM.BuildCall(builder, compiler.MethodLookup.GetMethod(methodRef), args, string.Empty);

            if(methodRef.ReturnType.MetadataType != MetadataType.Void)
            {
                compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(ret, methodRef.ReturnType.GetTypeInfo()));
            }

            // TODO: "this" parameter

            // TODO: tail
        }
    }
}
