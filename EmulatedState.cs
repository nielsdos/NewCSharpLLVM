using System;
using System.Collections.Generic;
using LLVMSharp;
using System.Diagnostics;

namespace CSharpLLVM
{
    public class EmulatedState
    {
        private List<EmulatedStateValue> evaluationStack = new List<EmulatedStateValue>();
        private List<EmulatedStateValue> evaluationStackAtStart;
        public EmulatedStateValue[] Locals { get; private set; }
        private EmulatedStateValue[] LocalsAtStart;

        public int StackSize { get { return evaluationStack.Count; } }

        public EmulatedState(int localCount)
        {
            Locals = new EmulatedStateValue[localCount];
            LocalsAtStart = new EmulatedStateValue[localCount];
        }

        public EmulatedState(LLVMBuilderRef builder, BasicBlock origin)
        {
            var state = origin.GetState();
            Locals = new EmulatedStateValue[state.Locals.Length];
            LocalsAtStart = new EmulatedStateValue[state.Locals.Length];
            for(int i = 0; i < Locals.Length; ++i)
            {
                var other = state.Locals[i];
                if(other != null)
                {
                    //Console.WriteLine("    Copying local " + i);
                    Locals[i] = new EmulatedStateValue(builder, origin, other);
                    LocalsAtStart[i] = Locals[i];
                }
            }

            foreach(var value in state.evaluationStack)
            {
                evaluationStack.Add(new EmulatedStateValue(builder, origin, value));
            }

            evaluationStackAtStart = new List<EmulatedStateValue>(evaluationStack);
        }

        public void StackPush(EmulatedStateValue value)
        {
            Debug.Assert(value != null);
            evaluationStack.Add(value);
        }

        public EmulatedStateValue StackPop()
        {
            int index = evaluationStack.Count - 1;
            EmulatedStateValue value = evaluationStack[index];
            evaluationStack.RemoveAt(index);
            return value;
        }

        public EmulatedStateValue StackPeek()
        {
            int index = evaluationStack.Count - 1;
            return evaluationStack[index];
        }

        public void Merge(LLVMBuilderRef builder, BasicBlock origin)
        {
            var otherState = origin.GetState();

            if(evaluationStackAtStart.Count != otherState.evaluationStack.Count)
                throw new InvalidOperationException("Cannot merge stacks with a difference in size");

            for(int i = 0; i < LocalsAtStart.Length; ++i)
            {
                if(otherState.Locals[i] != null && LocalsAtStart[i] != null)
                    LocalsAtStart[i].Merge(builder, origin, otherState.Locals[i]);
            }

            for(int i = 0; i < evaluationStackAtStart.Count; ++i)
            {
                evaluationStackAtStart[i].Merge(builder, origin, otherState.evaluationStack[i]);
            }
        }
    }
}
