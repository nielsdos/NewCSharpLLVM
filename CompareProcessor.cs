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
            // TODO: support correct typing
            
            var value2 = compiler.CurrentBasicBlock.GetState().StackPop();
            var value1 = compiler.CurrentBasicBlock.GetState().StackPop();

            var result = LLVM.BuildICmp(builder, GetIntPredicateFromCode(insn.OpCode.Code), value1.Value, value2.Value, string.Empty);
            compiler.CurrentBasicBlock.GetState().StackPush(new EmulatedStateValue(result));
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
    }
}
