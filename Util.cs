using Mono.Cecil.Cil;

namespace CSharpLLVM
{
    public static class Util
    {
        public static bool IsUnconditionalBranchInstruction(this Instruction insn)
        {
            switch(insn.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBranchInstruction(this Instruction insn)
        {
            switch(insn.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                case Code.Brfalse:
                case Code.Brfalse_S:
                case Code.Brtrue:
                case Code.Brtrue_S:
                //case Code.Call:
                //case Code.Calli:
                //case Code.Callvirt:
                case Code.Jmp:
                case Code.Leave:
                case Code.Leave_S:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsBlockTerminator(this Instruction insn)
        {
            switch(insn.OpCode.Code)
            {
                case Code.Ret:
                    return true;

                default:
                    return IsBranchInstruction(insn);
            }
        }
    }
}