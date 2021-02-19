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

        public void InheritState(LLVMBuilderRef builder, BasicBlock origin)
        {
            if(state == null)
            {
                state = new EmulatedState(builder, origin);
            }
            else
            {
                state.Merge(builder, origin);
            }
        }
    }
}
