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

        public EmulatedState(EmulatedState state, LLVMBuilderRef builder, BasicBlock origin)
        {
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
            if(evaluationStackAtStart.Count != otherState.evaluationStack.Count)
                throw new InvalidOperationException("Cannot merge stacks with a difference in size");

            for(int i = 0; i < Math.Max(Locals.Length, otherState.Locals.Length); ++i)
            {
                if(otherState.Locals[i] == null)
                    continue;

                //Console.WriteLine("    Inheriting local " + i);

                if(LocalsAtStart[i] != null)
                    LocalsAtStart[i].Merge(builder, mergingBasicBlock, otherState.Locals[i]);
            }

            for(int i = 0; i < evaluationStackAtStart.Count; ++i)
            {
                evaluationStackAtStart[i].Merge(builder, mergingBasicBlock, otherState.evaluationStack[i]);
            }
        }
    }
}
