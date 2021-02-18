using LLVMSharp;

namespace CSharpLLVM
{
    public class BasicBlock
    {
        public LLVMBasicBlockRef LLVMBlock { get; private set; }
        private EmulatedState state;
        private MethodCompiler compiler;

        public BasicBlock(MethodCompiler compiler, LLVMValueRef fn, string name)
        {
            this.compiler = compiler;
            this.LLVMBlock = LLVM.AppendBasicBlock(fn, name);
            // By default no state is assigned yet.
            this.state = null;
        }

        public EmulatedState GetState()
        {
            if(state == null)
                state = new EmulatedState(compiler.MethodDef.Body.Variables.Count);
            return state;
        }

        public void InheritState(LLVMBuilderRef builder, BasicBlock origin, EmulatedState inheritedState)
        {
            if(state == null)
            {
                state = new EmulatedState(inheritedState, builder, origin);
            }
            else
            {
                state.Merge(builder, origin, inheritedState);
            }
        }
    }
}
