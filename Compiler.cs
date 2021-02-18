using System;
using LLVMSharp;
using Mono.Cecil;
using System.Runtime.InteropServices;

namespace CSharpLLVM
{
    public class Compiler
    {
        private AssemblyDefinition assemblyDefinition;
        public LLVMModuleRef ModuleRef { get; private set; }
        public InstructionProcessorDispatcher InstructionProcessorDispatcher { get; private set; }

        public Compiler(AssemblyDefinition assemblyDefinition)
        {
            this.InstructionProcessorDispatcher = new InstructionProcessorDispatcher();
            this.assemblyDefinition = assemblyDefinition;
            this.ModuleRef = LLVM.ModuleCreateWithName("module");
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
            LLVM.InitializeFunctionPassManager(fnPassManager);

            // Optimizations
            LLVM.AddPromoteMemoryToRegisterPass(fnPassManager);
            LLVM.AddInstructionCombiningPass(fnPassManager);
            LLVM.AddJumpThreadingPass(fnPassManager);
            LLVM.AddGVNPass(fnPassManager);
            LLVM.AddEarlyCSEPass(fnPassManager);
            LLVM.AddConstantPropagationPass(fnPassManager);
            LLVM.AddCFGSimplificationPass(fnPassManager);

            foreach(ModuleDefinition moduleDef in assemblyDefinition.Modules)
            {
                Console.WriteLine(moduleDef.Name);

                foreach(TypeDefinition typeDef in moduleDef.Types)
                {
                    Console.WriteLine("  " + typeDef.Name);

                    foreach(MethodDefinition methodDef in typeDef.Methods)
                    {
                        var methodCompiler = new MethodCompiler(this, methodDef);
                        methodCompiler.Compile();

                        // TODO: make better
                        if(!LLVM.VerifyFunction(methodCompiler.FunctionValueRef, LLVMVerifierFailureAction.LLVMPrintMessageAction))
                            LLVM.RunFunctionPassManager(fnPassManager, methodCompiler.FunctionValueRef);
                    }
                }
            }

            // TODO: handle return value somehow
            LLVM.VerifyModule(ModuleRef, LLVMVerifierFailureAction.LLVMPrintMessageAction, out var outMsg);

            var passManager = LLVM.CreatePassManager();

            LLVM.RunPassManager(passManager, ModuleRef);

            // Cleanup
            LLVM.DisposePassManager(passManager);
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

            LLVM.DisposeTargetMachine(machineRef);
        }
    }
}
