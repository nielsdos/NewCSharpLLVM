using System;
using LLVMSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace CSharpLLVM
{
    public class MethodCompiler
    {
        public MethodDefinition MethodDef { get; private set; }
        public LLVMValueRef FunctionValueRef { get; private set; }
        private Compiler compiler;

        public TypeLookup TypeLookup { get { return compiler.TypeLookup; } }
        public MethodLookup MethodLookup { get { return compiler.MethodLookup; } }
        public StaticFieldLookup StaticFieldLookup { get { return compiler.StaticFieldLookup; } }

        private Dictionary<int, BasicBlock> offsetToBasicBlock = new Dictionary<int, BasicBlock>();

        private Dictionary<int, HashSet<int>> outgoingEdges = new Dictionary<int, HashSet<int>>();

        public EmulatedStateValue[] ArgumentValues { get; private set; }
        public EmulatedStateValue[] LocalValues { get; private set; }

        public BasicBlock CurrentBasicBlock { get; private set; }
        private HashSet<int> processedBlocks = new HashSet<int>();

        public MethodCompiler(Compiler compiler, MethodDefinition methodDef)
        {
            this.compiler = compiler;
            this.MethodDef = methodDef;
            this.FunctionValueRef = MethodLookup.DeclareMethod(methodDef);
        }

        private void AddOutgoingEdge(int from, int to)
        {
            //Console.WriteLine("edge " + from.ToString("x") + " -> " + to.ToString("x"));
            if(outgoingEdges.TryGetValue(from, out var set))
            {
                set.Add(to);
            }
            else
            {
                var newSet = new HashSet<int>();
                outgoingEdges.Add(from, newSet);
                newSet.Add(to);
            }
        }

        private void AddBasicBlockAt(Instruction insn)
        {
            int id = insn.Offset;
            if (!offsetToBasicBlock.ContainsKey(id))
            {
                offsetToBasicBlock.Add(id, new BasicBlock(this, FunctionValueRef, insn, "IL_" + id.ToString("x")));
            }
        }

        public void Compile()
        {
            if (offsetToBasicBlock.Count > 0)
                throw new InvalidOperationException("This method is already compiled.");

            LLVMBuilderRef builder = LLVM.CreateBuilder();

            // Add the entry point as the first basic block.
            {
                var it = MethodDef.Body.Instructions.GetEnumerator();
                it.MoveNext();
                AddBasicBlockAt(it.Current);
            }

            // Generate the entry point code for arguments.
            LLVM.PositionBuilderAtEnd(builder, offsetToBasicBlock[0].LLVMBlock);
            uint paramCount = LLVM.CountParams(FunctionValueRef);
            ArgumentValues = new EmulatedStateValue[paramCount];
            int offset = MethodDef.HasThis ? 1 : 0;
            for(uint i = 0; i < paramCount; ++i)
            {
                LLVMValueRef param = LLVM.GetParam(FunctionValueRef, i);
                var valueRef = LLVM.BuildAlloca(builder, LLVM.TypeOf(param), "arg" + i);

                TypeInfo typeInfo;
                if(MethodDef.HasThis && i == 0)
                {
                    typeInfo = TypeInfo.Reference;
                }
                else
                {
                    typeInfo = MethodDef.Parameters[(int) i - offset].ParameterType.GetTypeInfo();
                }

                ArgumentValues[i] = new EmulatedStateValue(valueRef, typeInfo);
                LLVM.BuildStore(builder, param, valueRef);
            }

            // Generate the entry point code for locals.
            {
                var locals = MethodDef.Body.Variables;
                LocalValues = new EmulatedStateValue[locals.Count];
                for(int i = 0; i < locals.Count; ++i)
                {
                    var cilType = locals[i].VariableType;
                    var valueRef = LLVM.BuildAlloca(builder, TypeLookup.GetLLVMTypeRef(cilType), "local" + i);
                    LocalValues[i] = new EmulatedStateValue(valueRef, cilType.GetTypeInfo());
                }
            }

            // Now, find the basic blocks.
            {
                int currentBlockIndex = 0;
                foreach(Instruction insn in MethodDef.Body.Instructions)
                {
                    if(HasBasicBlock(insn.Offset))
                    {
                        // This is a fallthrough?
                        if(currentBlockIndex != insn.Offset)
                        {
                            // Can't be fallthrough if previous instruction was an unconditional branch.
                            if(!insn.Previous.IsUnconditionalBranchInstruction())
                                AddOutgoingEdge(currentBlockIndex, insn.Offset);
                        }

                        currentBlockIndex = insn.Offset;
                    }

                    if(insn.IsBranchInstruction())
                    {
                        AddBasicBlockAt((Instruction) insn.Operand);
                        AddOutgoingEdge(currentBlockIndex, ((Instruction) insn.Operand).Offset);
                        AddBasicBlockAt(insn.Next);
                        // Can't be fallthrough if current instruction was an unconditional branch.
                        if(!insn.IsUnconditionalBranchInstruction())
                            AddOutgoingEdge(currentBlockIndex, insn.Next.Offset);
                    }
                }
            }

            compileBasicBlock(builder, 0);

            LLVM.DisposeBuilder(builder);
        }

        private void compileBasicBlock(LLVMBuilderRef builder, int currentBlockIndex) {
            Console.WriteLine("           process " + currentBlockIndex.ToString("x"));
            CurrentBasicBlock = GetBasicBlock(currentBlockIndex);
            LLVM.PositionBuilderAtEnd(builder, CurrentBasicBlock.LLVMBlock);

            processedBlocks.Add(currentBlockIndex);

            Instruction insn = CurrentBasicBlock.FirstInstructionRef;
            while(true)
            {
                CompileInstruction(insn, builder);

                insn = insn.Next;
                if(insn == null)
                    break;

                // This basic block has ended, switch to another if required.
                if(GetBasicBlock(insn.Offset, out var basicBlock))
                {
                    // If we have to terminate the old basic block explicitely because we created it implicitly, do so.
                    if(!insn.Previous.IsBlockTerminator())
                    {
                        LLVM.BuildBr(builder, basicBlock.LLVMBlock);
                    }

                    // Inherit state to outgoing edges.
                    if(outgoingEdges.TryGetValue(currentBlockIndex, out var destinations))
                    {
                        foreach(int destination in destinations)
                        {
                            var destinationBlock = GetBasicBlock(destination);
                            LLVM.PositionBuilderAtEnd(builder, destinationBlock.LLVMBlock);
                            Console.WriteLine("  Inherit from " + currentBlockIndex.ToString("x") + " -> " + destination.ToString("x"));
                            destinationBlock.InheritState(builder, CurrentBasicBlock);
                        }

                        foreach(int destination in destinations)
                        {
                            if(!processedBlocks.Contains(destination))
                            {
                                compileBasicBlock(builder, destination);
                            }
                        }
                    }

                    break;
                }
            }
        }

        public bool HasBasicBlock(int offset)
        {
            return offsetToBasicBlock.ContainsKey(offset);
        }

        public BasicBlock GetBasicBlock(int offset)
        {
            return offsetToBasicBlock[offset];
        }

        public bool GetBasicBlock(int offset, out BasicBlock basicBlock)
        {
            return offsetToBasicBlock.TryGetValue(offset, out basicBlock);
        }

        private void CompileInstruction(Instruction insn, LLVMBuilderRef builder)
        {
            Console.WriteLine("  compile " + insn);
            compiler.InstructionProcessorDispatcher.Process(this, insn, builder);
        }
    }
}
