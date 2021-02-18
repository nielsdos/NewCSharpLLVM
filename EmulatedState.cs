using System;
using System.Collections.Generic;
using LLVMSharp;

namespace CSharpLLVM
{
    public class EmulatedState
    {
        private List<EmulatedStateValue> evaluationStack = new List<EmulatedStateValue>();
        public EmulatedStateValue[] Locals { get; private set; }

        public int StackSize { get { return evaluationStack.Count; } }

        public EmulatedState(int localCount)
        {
            Locals = new EmulatedStateValue[localCount];
        }

        public EmulatedState(EmulatedState state)
        {
            Locals = new EmulatedStateValue[state.Locals.Length];
            for(int i = 0; i < Locals.Length; ++i)
            {
                var other = state.Locals[i];
                if(other != null)
                    Locals[i] = new EmulatedStateValue(other);
            }

            foreach(var value in state.evaluationStack)
            {
                evaluationStack.Add(new EmulatedStateValue(value));
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

        public void Merge(LLVMBuilderRef builder, LLVMBasicBlockRef mergingBasicBlock, EmulatedState otherState)
        {
            if(evaluationStack.Count != otherState.evaluationStack.Count)
                throw new InvalidOperationException("Cannot merge stacks with a difference in size");

            for(int i = 0; i < Math.Max(Locals.Length, otherState.Locals.Length); ++i)
            {
                if(otherState.Locals[i] == null) continue;

                if(Locals[i] != null)
                    Locals[i].Merge(builder, mergingBasicBlock, otherState.Locals[i]);
                else if(Locals[i] == null)
                    Locals[i] = new EmulatedStateValue(otherState.Locals[i]);
            }

            for(int i = 0; i < evaluationStack.Count; ++i)
            {
                evaluationStack[i].Merge(builder, mergingBasicBlock, otherState.evaluationStack[i]);
            }
        }
    }
}
