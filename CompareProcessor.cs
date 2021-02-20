using LLVMSharp;
using Mono.Cecil.Cil;
using System;

namespace CSharpLLVM
{
    [InstructionHandler(Code.Clt, Code.Clt_Un, Code.Ceq, Code.Cgt, Code.Cgt_Un)]
    public class CompareProcessor : InstructionProcessor
    {
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            var value2 = compiler.CurrentBasicBlock.GetState().StackPop();
            var value1 = compiler.CurrentBasicBlock.GetState().StackPop();

            LLVMValueRef result;
            if(value1.TypeInfo == TypeInfo.FloatingPrimitive)
                result = LLVM.BuildFCmp(builder, GetFloatPredicateFromCode(insn.OpCode.Code), value1.Value, value2.Value, string.Empty);
            else
                result = LLVM.BuildICmp(builder, GetIntPredicateFromCode(insn.OpCode.Code), value1.Value, value2.Value, string.Empty);

            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result, TypeInfo.IntegralPrimitive));
        }

        private LLVMIntPredicate GetIntPredicateFromCode(Code code)
        {
            switch (code)
            {
                case Code.Clt:
                    return LLVMIntPredicate.LLVMIntSLT;

                case Code.Clt_Un:
                    return LLVMIntPredicate.LLVMIntULT;

                case Code.Ceq:
                    return LLVMIntPredicate.LLVMIntEQ;

                case Code.Cgt:
                    return LLVMIntPredicate.LLVMIntSGT;

                case Code.Cgt_Un:
                    return LLVMIntPredicate.LLVMIntUGT;

                default:
                    throw new InvalidOperationException("Unexpected code " + code);
            }
        }

        private LLVMRealPredicate GetFloatPredicateFromCode(Code code)
        {
            switch (code)
            {
                case Code.Clt:
                    return LLVMRealPredicate.LLVMRealOLT;

                case Code.Clt_Un:
                    return LLVMRealPredicate.LLVMRealULT;

                case Code.Ceq:
                    return LLVMRealPredicate.LLVMRealOEQ;

                case Code.Cgt:
                    return LLVMRealPredicate.LLVMRealOGT;

                case Code.Cgt_Un:
                    return LLVMRealPredicate.LLVMRealUGT;

                default:
                    throw new InvalidOperationException("Unexpected code " + code);
            }
        }
    }
}
