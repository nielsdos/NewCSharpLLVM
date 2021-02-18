using System;
using System.Collections.Generic;
using LLVMSharp;

namespace CSharpLLVM
{
    public class EmulatedState
    {
        private List<EmulatedStateValue> evaluationStack = new List<EmulatedStateValue>();
        public EmulatedStateValue[] Locals { get; private set; }
        private EmulatedStateValue[] LocalsBeforeAnyInstructionIsExecuted;

        public int StackSize { get { return evaluationStack.Count; } }

        public EmulatedState(int localCount)
        {
            Locals = new EmulatedStateValue[localCount];
            LocalsBeforeAnyInstructionIsExecuted = new EmulatedStateValue[localCount];
        }

        public EmulatedState(EmulatedState state, LLVMBuilderRef builder, BasicBlock origin)
        {
            Locals = new EmulatedStateValue[state.Locals.Length];
            LocalsBeforeAnyInstructionIsExecuted = new EmulatedStateValue[state.Locals.Length];
            for(int i = 0; i < Locals.Length; ++i)
            {
                var other = state.Locals[i];
                if(other != null/* && other.Origin == origin*/)
                {
                    Console.WriteLine("    Copying local " + i);
                    Locals[i] = new EmulatedStateValue(builder, other);
                    LocalsBeforeAnyInstructionIsExecuted[i] = Locals[i];
                }
            }

            foreach(var value in state.evaluationStack)
            {
                evaluationStack.Add(new EmulatedStateValue(builder, value));
            }
        }

        public void StackPush(EmulatedStateValue value)
        {
            evaluationStack.Add(value);
        }

        public EmulatedStateValue StackPop()
        {
            int index = evaluationStack.Count - 1;
            EmulatedStateValue value = evaluationStack[index];
            evaluationStack.RemoveAt(index);
            return value;
        }

        public int LocalCount()
        {
            int count = 0;
            for(int i = 0; i < Locals.Length; ++i)
            {
                if(Locals[i] != null)
                    ++count;
            }
            return count;
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock mergingBasicBlock, EmulatedState otherState)
        {
            if(evaluationStack.Count != otherState.evaluationStack.Count)
                throw new InvalidOperationException("Cannot merge stacks with a difference in size");

            for(int i = 0; i < Math.Max(Locals.Length, otherState.Locals.Length); ++i)
            {
                if(otherState.Locals[i] == null)
                    continue;

                Console.WriteLine("    Inheriting local " + i);

                // TODO: similar to "LocalsBeforeAnyInstructionIsExecuted", we should handle the stack as this as well?
                // XXX: or find a diff solution
                if(LocalsBeforeAnyInstructionIsExecuted[i] != null)
                    LocalsBeforeAnyInstructionIsExecuted[i].Merge(builder, mergingBasicBlock, otherState.Locals[i]);
                else if(LocalsBeforeAnyInstructionIsExecuted[i] == null)
                    LocalsBeforeAnyInstructionIsExecuted[i] = new EmulatedStateValue(builder, otherState.Locals[i]);
            }

            for(int i = 0; i < evaluationStack.Count; ++i)
            {
                evaluationStack[i].Merge(builder, mergingBasicBlock, otherState.evaluationStack[i]);
            }
        }
    }
}
