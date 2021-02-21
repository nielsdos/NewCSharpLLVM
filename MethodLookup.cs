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
        // TODO: can we use something better than string?
        private Dictionary<string, LLVMValueRef> methodMap = new Dictionary<string, LLVMValueRef>();
        private LLVMModuleRef moduleRef;
        private TypeLookup typeLookup;

        private LLVMBuilderRef cctorCallBuilder;

        public MethodLookup(LLVMModuleRef moduleRef, TypeLookup typeLookup)
        {
            this.moduleRef = moduleRef;
            this.typeLookup = typeLookup;
            createSystemObjectCtor();
            createCctorCaller();
        }

        ~MethodLookup()
        {
            LLVM.DisposeBuilder(cctorCallBuilder);
        }

        public void Finish()
        {
            LLVM.BuildRetVoid(cctorCallBuilder);
        }

        private void createCctorCaller()
        {
            cctorCallBuilder = LLVM.CreateBuilder();
            var fnType = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[0], false);
            var fn = LLVM.AddFunction(moduleRef, "cctor_caller", fnType);
            var basicBlock = LLVM.AppendBasicBlock(fn, string.Empty);
            LLVM.PositionBuilderAtEnd(cctorCallBuilder, basicBlock);
        }

        private void createSystemObjectCtor()
        {
            var paramTypes = new LLVMTypeRef[] { LLVM.PointerType(LLVM.VoidType(), 0) };
            var fnType = LLVM.FunctionType(LLVM.VoidType(), paramTypes, false);
            var valueRef = LLVM.AddFunction(moduleRef, string.Empty, fnType);

            // TODO: maybe put this somewhere else?
            // TODO: this is somewhat a workaround?
            /*var asmDef = AssemblyDefinition.ReadAssembly(typeof(object).Assembly.Location);
            var sysObj = asmDef.MainModule.GetTypes().Where(typeDef => typeDef.FullName == "System.Object").Single();
            var methodDef = sysObj.Methods.Where(methodDef => methodDef.Name == ".ctor").Single();
            System.Console.WriteLine(methodDef);
            methodMap.Add(methodDef., valueRef);*/
            methodMap.Add("System.Void System.Object::.ctor()", valueRef);
            
            var builder = LLVM.CreateBuilder();
            var block = LLVM.AppendBasicBlock(valueRef, string.Empty);
            LLVM.PositionBuilderAtEnd(builder, block);
            LLVM.BuildRetVoid(builder);
            LLVM.DisposeBuilder(builder);
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
                if(methodDef.DeclaringType.IsValueType)
                    paramTypes[0] = LLVM.PointerType(paramTypes[0], 0);
            }

            var fnType = LLVM.FunctionType(
                typeLookup.GetLLVMTypeRef(methodDef.MethodReturnType.ReturnType),
                paramTypes,
                false
            );

            var valueRef = LLVM.AddFunction(moduleRef, methodDef.FullName, fnType);
            methodMap.Add(methodDef.FullName, valueRef);

            if(methodDef.IsConstructor && methodDef.IsStatic)
            {
                LLVM.SetLinkage(valueRef, LLVMLinkage.LLVMInternalLinkage);
                LLVM.BuildCall(cctorCallBuilder, valueRef, new LLVMValueRef[0], string.Empty);
            }

            return valueRef;
        }

        /// <summary>
        /// Gets the method LLVM value reference.
        /// </summary>
        /// <param name="methodRef">The CIL method reference</param>
        /// <returns>The LLVM value reference</returns>
        public LLVMValueRef GetMethod(MethodReference methodRef)
        {
            return methodMap[methodRef.FullName];
        }
    }
}
