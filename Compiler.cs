using System;
using LLVMSharp;
using Mono.Cecil;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace CSharpLLVM
{
    public class Compiler
    {
        private AssemblyDefinition assemblyDefinition;
        public LLVMModuleRef ModuleRef { get; private set; }
        public InstructionProcessorDispatcher InstructionProcessorDispatcher { get; private set; }
        public TypeLookup TypeLookup { get; private set; }
        public MethodLookup MethodLookup { get; private set; }

        public Compiler(AssemblyDefinition assemblyDefinition)
        {
            this.assemblyDefinition = assemblyDefinition;
            this.ModuleRef = LLVM.ModuleCreateWithName("module");
            this.InstructionProcessorDispatcher = new InstructionProcessorDispatcher();
            this.TypeLookup = new TypeLookup();
            this.MethodLookup = new MethodLookup(ModuleRef, TypeLookup);
        }

        public void Compile()
        {
            string triple = "x86_64-pc-linux-gnu";

            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();

            LLVM.SetTarget(ModuleRef, triple);

            var fnPassManager = LLVM.CreateFunctionPassManagerForModule(ModuleRef);
            var modulePassManager = LLVM.CreatePassManager();
            LLVM.InitializeFunctionPassManager(fnPassManager);

            // Optimizations
            LLVM.AddPromoteMemoryToRegisterPass(fnPassManager);
            LLVM.AddInstructionCombiningPass(fnPassManager);
            LLVM.AddJumpThreadingPass(fnPassManager);
            LLVM.AddEarlyCSEPass(fnPassManager);
            LLVM.AddConstantPropagationPass(fnPassManager);
            LLVM.AddCFGSimplificationPass(fnPassManager);
            LLVM.AddReassociatePass(fnPassManager);
            LLVM.AddLoopUnrollPass(fnPassManager);
            LLVM.AddLoopRerollPass(fnPassManager);
            LLVM.AddLoopDeletionPass(fnPassManager);
            LLVM.AddLoopRotatePass(fnPassManager);
            LLVM.AddLoopIdiomPass(fnPassManager);
            LLVM.AddLoopUnswitchPass(fnPassManager);
            LLVM.AddDeadStoreEliminationPass(fnPassManager);
            LLVM.AddBasicAliasAnalysisPass(fnPassManager);
            LLVM.AddIndVarSimplifyPass(fnPassManager);
            LLVM.AddSCCPPass(fnPassManager);
            LLVM.AddLICMPass(fnPassManager);
            LLVM.AddCorrelatedValuePropagationPass(fnPassManager);
            LLVM.AddScopedNoAliasAAPass(modulePassManager);
            LLVM.AddDeadArgEliminationPass(modulePassManager);
            LLVM.AddTailCallEliminationPass(modulePassManager);
            LLVM.AddLoopUnswitchPass(modulePassManager);
            LLVM.AddIPSCCPPass(modulePassManager);
            LLVM.AddReassociatePass(modulePassManager);
            LLVM.AddAlwaysInlinerPass(modulePassManager);

            // O2
            LLVM.AddNewGVNPass(fnPassManager);
            LLVM.AddLowerExpectIntrinsicPass(fnPassManager);
            LLVM.AddScalarReplAggregatesPassSSA(fnPassManager);
            LLVM.AddMergedLoadStoreMotionPass(fnPassManager);
            LLVM.AddSLPVectorizePass(fnPassManager);
            LLVM.AddConstantMergePass(modulePassManager);
            LLVM.AddConstantMergePass(modulePassManager);

            // First, declare all types and only define the types in a later pass.
            // The reason is that we may have cycles of types.
            foreach(ModuleDefinition moduleDef in assemblyDefinition.Modules)
            {
                foreach(TypeDefinition typeDef in moduleDef.Types)
                    TypeLookup.DeclareType(typeDef);
            }

            // Now, we have all types, so we can declare the methods.
            // Again, we may have cycles so the methods are declared and defined in separate passes.
            // Also define the types now.
            var methodCompilerLookup = new Dictionary<MethodDefinition, MethodCompiler>();
            // TODO: parallellize this
            foreach(ModuleDefinition moduleDef in assemblyDefinition.Modules)
            {
                foreach(TypeDefinition typeDef in moduleDef.Types)
                {
                    TypeLookup.DefineType(typeDef);

                    foreach(MethodDefinition methodDef in typeDef.Methods)
                        methodCompilerLookup.Add(methodDef, new MethodCompiler(this, methodDef));
                }
            }

            // Compile the methods.
            foreach(ModuleDefinition moduleDef in assemblyDefinition.Modules)
            {
                foreach(TypeDefinition typeDef in moduleDef.Types)
                {
                    foreach(MethodDefinition methodDef in typeDef.Methods)
                    {
                        var methodCompiler = methodCompilerLookup[methodDef];
                        methodCompiler.Compile();

                        if(!LLVM.VerifyFunction(methodCompiler.FunctionValueRef, LLVMVerifierFailureAction.LLVMPrintMessageAction))
                            LLVM.RunFunctionPassManager(fnPassManager, methodCompiler.FunctionValueRef);
                    }
                }
            }

            // Free memory already.
            methodCompilerLookup.Clear();

            Console.WriteLine("-------------------");

            if(LLVM.VerifyModule(ModuleRef, LLVMVerifierFailureAction.LLVMPrintMessageAction, out var outMsg))
            {
                Console.WriteLine(outMsg);
            }

            LLVM.RunPassManager(modulePassManager, ModuleRef);

            // Cleanup
            LLVM.DisposePassManager(modulePassManager);
            LLVM.DumpModule(ModuleRef);




            // XXX

            string errorMsg;
            LLVMTargetRef targetRef;
            if(LLVM.GetTargetFromTriple(triple, out targetRef, out errorMsg))
            {
                Console.WriteLine(errorMsg);
            }

            LLVMTargetMachineRef machineRef = LLVM.CreateTargetMachine(targetRef, triple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);

            System.IO.File.Delete("test.s");            
            if(LLVM.TargetMachineEmitToFile(machineRef, ModuleRef, Marshal.StringToHGlobalAnsi("test.s"), LLVMCodeGenFileType.LLVMAssemblyFile, out errorMsg))
            {
                Console.WriteLine(errorMsg);
            }

            Console.WriteLine("written");

            LLVM.DisposeTargetMachine(machineRef);
        }
    }
}
