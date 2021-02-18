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

        private Dictionary<int, BasicBlock> offsetToBasicBlock = new Dictionary<int, BasicBlock>();

        private Dictionary<int, HashSet<int>> outgoingEdges = new Dictionary<int, HashSet<int>>();

        public LLVMValueRef[] ArgumentValues { get; private set; }

        public BasicBlock CurrentBasicBlock { get; private set; }

        public MethodCompiler(Compiler compiler, MethodDefinition methodDefinition)
        {
            this.compiler = compiler;
            this.MethodDef = methodDefinition;
            // TODO
            LLVMTypeRef[] paramTypes = { LLVM.Int32Type(), LLVM.Int32Type() };
            var fnType = LLVM.FunctionType(LLVM.Int32Type(), paramTypes, false);
            this.FunctionValueRef = LLVM.AddFunction(compiler.ModuleRef, methodDefinition.FullName, fnType);
        }

        private void AddOutgoingEdge(int from, int to)
        {
            Console.WriteLine("edge " + from.ToString("x") + " -> " + to.ToString("x"));
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

        private void AddBasicBlock(int id)
        {
            if (!offsetToBasicBlock.ContainsKey(id))
            {
                offsetToBasicBlock.Add(id, new BasicBlock(this, FunctionValueRef, "IL_" + id.ToString("x")));
            }
        }

        public void Compile()
        {
            if (offsetToBasicBlock.Count > 0)
                throw new InvalidOperationException("This method is already compiled.");

            LLVMBuilderRef builder = LLVM.CreateBuilder();

            // Add the entry point as the first basic block.
            AddBasicBlock(0);

            // Generate the entry point code.
            // This includes the setup for arguments.
            LLVM.PositionBuilderAtEnd(builder, offsetToBasicBlock[0].LLVMBlock);
            uint paramCount = LLVM.CountParams(FunctionValueRef);
            ArgumentValues = new LLVMValueRef[paramCount];
            for(uint i = 0; i < paramCount; ++i)
            {
                LLVMValueRef param = LLVM.GetParam(FunctionValueRef, i);
                ArgumentValues[i] = LLVM.BuildAlloca(builder, LLVM.TypeOf(param), "arg" + i);
                LLVM.BuildStore(builder, param, ArgumentValues[i]);
            }

            // Now, find the basic blocks.
            {
                // TODO: should be done in two passes?
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
                        AddBasicBlock(((Instruction) insn.Operand).Offset);
                        AddOutgoingEdge(currentBlockIndex, ((Instruction) insn.Operand).Offset);
                        AddBasicBlock(insn.Next.Offset);
                        // Can't be fallthrough if current instruction was an unconditional branch.
                        if(!insn.IsUnconditionalBranchInstruction())
                            AddOutgoingEdge(currentBlockIndex, insn.Next.Offset);
                    }

                    //Console.WriteLine(insn);
                }
            }

            lol(builder, 0);

            LLVM.DisposeBuilder(builder);
        }

        private HashSet<int> processedBlocks = new HashSet<int>();

        private void lol(LLVMBuilderRef builder, int currentBlockIndex) {
            Console.WriteLine("           process " + currentBlockIndex.ToString("x"));
            CurrentBasicBlock = GetBasicBlock(currentBlockIndex);
            LLVM.PositionBuilderAtEnd(builder, CurrentBasicBlock.LLVMBlock);

            processedBlocks.Add(currentBlockIndex);

            
            foreach(Instruction insn in MethodDef.Body.Instructions)
            {
                // TODO: XXX shitty
                if(insn.Offset < currentBlockIndex) continue;

                // TODO: also a shitty check...
                // This basic block has ended, switch to another if required.
                if(insn.Offset > currentBlockIndex && GetBasicBlock(insn.Offset, out var basicBlock))
                {
                    // If we have to terminate the old basic block explicitely because we created it implicitly, do so.
                    if(!insn.Previous.IsBlockTerminator())
                    {
                        LLVM.BuildBr(builder, basicBlock.LLVMBlock);
                    }

                    // Activate new basic block.
                    //LLVM.PositionBuilderAtEnd(builder, basicBlock.LLVMBlock);

                    Console.WriteLine("I'm in block " + currentBlockIndex.ToString("x"));
                    Console.WriteLine("Locals left: " + CurrentBasicBlock.GetState().LocalCount());

                    // Inherit state to outgoing edges.
                    if(outgoingEdges.TryGetValue(currentBlockIndex, out var destinations))
                    {
                        foreach(int destination in destinations)
                        {
                            // TODO: actually, this should be merged into its own method, such that the loops are merged
                            // and the position code is not duplicated...
                            var destinationBlock = GetBasicBlock(destination);
                            LLVM.PositionBuilderAtEnd(builder, destinationBlock.LLVMBlock);
                            Console.WriteLine("  Inherit from " + currentBlockIndex.ToString("x") + " -> " + destination.ToString("x"));
                            destinationBlock.InheritState(builder, CurrentBasicBlock, CurrentBasicBlock.GetState());
                        }
                    }

                    // TODO: code duplication
                    if(outgoingEdges.TryGetValue(currentBlockIndex, out var destinations2))
                    {
                        foreach(int destination in destinations2)
                        {
                            if(!processedBlocks.Contains(destination))
                            {
                                lol(builder, destination);
                            }
                        }
                    }

                    //currentBlockIndex = insn.Offset;
                    //CurrentBasicBlock = basicBlock;
                    break;
                }

                LLVM.PositionBuilderAtEnd(builder, CurrentBasicBlock.LLVMBlock);//TODO: shitty
                CompileInstruction(insn, builder);

                //Console.WriteLine("  stack count after insn: " + CurrentBasicBlock.GetState().StackSize);
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
