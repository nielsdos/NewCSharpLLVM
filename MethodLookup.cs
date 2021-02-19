using System.Collections.Generic;
using Mono.Cecil;
using LLVMSharp;

namespace CSharpLLVM
{
    /// <summary>
    /// Keeps track of the CIL methods -> LLVM value refs.
    /// </summary>
    public sealed class MethodLookup
    {
        private Dictionary<MethodReference, LLVMValueRef> methodMap = new Dictionary<MethodReference, LLVMValueRef>();
        private LLVMModuleRef moduleRef;
        private TypeLookup typeLookup;

        public MethodLookup(LLVMModuleRef moduleRef, TypeLookup typeLookup)
        {
            // TODO: make an empty fn for System.Void System.Object::.ctor()
            this.moduleRef = moduleRef;
            this.typeLookup = typeLookup;
        }

        /// <summary>
        /// Declares a method.
        /// </summary>
        /// <param name="methodDef">The CIL method definition</param>
        /// <returns>The LLVM value reference</returns>
        public LLVMValueRef DeclareMethod(MethodDefinition methodDef)
        {
            int offset = methodDef.HasThis ? 1 : 0;

            var paramTypes = new LLVMTypeRef[methodDef.Parameters.Count + offset];
            for(int i = 0; i < paramTypes.Length - offset; ++i)
            {
                paramTypes[i + offset] = typeLookup.GetLLVMTypeRef(methodDef.Parameters[i].ParameterType);
            }

            if(methodDef.HasThis)
            {
                paramTypes[0] = typeLookup.GetLLVMTypeRef(methodDef.DeclaringType);
            }

            var fnType = LLVM.FunctionType(
                typeLookup.GetLLVMTypeRef(methodDef.MethodReturnType.ReturnType),
                paramTypes,
                false
            );

            var valueRef = LLVM.AddFunction(moduleRef, methodDef.FullName, fnType);
            methodMap.Add(methodDef, valueRef);

            return valueRef;
        }

        /// <summary>
        /// Gets the method LLVM value reference.
        /// </summary>
        /// <param name="methodRef">The CIL method reference</param>
        /// <returns>The LLVM value reference</returns>
        public LLVMValueRef GetMethod(MethodReference methodRef)
        {
            return methodMap[methodRef];
        }
    }
}
