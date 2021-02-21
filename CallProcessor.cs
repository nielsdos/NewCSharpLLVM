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

            int offset = methodRef.HasThis ? 1 : 0;
            int argCount = methodRef.Parameters.Count + offset;
            LLVMValueRef[] args = new LLVMValueRef[argCount];
            for(int i = 0; i < argCount; ++i)
            {
                args[argCount - i - 1] = compiler.CurrentBasicBlock.GetState().StackPop().Value;
            }

            if(methodRef.HasThis)
            {
                // TODO: don't always do this
                var newType = compiler.TypeLookup.GetLLVMTypeRef(methodRef.DeclaringType);
                if(methodRef.DeclaringType.IsValueType)
                    newType = LLVM.PointerType(newType, 0);
                args[0] = LLVM.BuildPointerCast(builder, args[0], newType, string.Empty);
            }

            var ret = LLVM.BuildCall(builder, compiler.MethodLookup.GetMethod(methodRef), args, string.Empty);

            if(insn.HasPrefix(Code.Tail))
                LLVM.SetTailCall(ret, true);

            if(methodRef.ReturnType.MetadataType != MetadataType.Void)
            {
                compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(ret, methodRef.ReturnType.GetTypeInfo()));
            }
        }
    }
}
