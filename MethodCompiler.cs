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
            offsetToBasicBlock.Add(0, new BasicBlock(this, FunctionValueRef, "IL_0"));

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
                        if(currentBlockIndex != insn.Offset) {
                            if(!insn.IsUnconditionalBranchInstruction())
                                AddOutgoingEdge(currentBlockIndex, insn.Offset);
                        }

                        currentBlockIndex = insn.Offset;
                    }

                    if(insn.IsBranchInstruction())
                    {
                        AddBasicBlock(((Instruction) insn.Operand).Offset);
                        AddOutgoingEdge(currentBlockIndex, ((Instruction) insn.Operand).Offset);
                        AddBasicBlock(insn.Next.Offset);
                        // If this is an unconditional branch, the next instruction is not reachable from this block.
                        if(insn.IsUnconditionalBranchInstruction())
                            AddOutgoingEdge(currentBlockIndex, insn.Next.Offset);
                    }

                    Console.WriteLine(insn);
                }
            }

            // Now, generate the instructions.
            {
                int currentBlockIndex = 0; // TODO: put the offset in the basic block object?
                foreach(Instruction insn in MethodDef.Body.Instructions)
                {
                    // This basic block has ended, switch to another if required.
                    if(GetBasicBlock(insn.Offset, out var basicBlock))
                    {
                        // `CurrentBasicBlock` can be null, because this will be executed for IL_0 as well.

                        // If we have to terminate the old basic block explicitely because we created it implicitly, do so.
                        if(CurrentBasicBlock != null && !insn.Previous.IsBlockTerminator())
                        {
                            // TODO: will be incorrect when we change this to loop in a different order
                            LLVM.BuildBr(builder, basicBlock.LLVMBlock);
                        }

                        // Activate new basic block.
                        LLVM.PositionBuilderAtEnd(builder, basicBlock.LLVMBlock);

                        if(CurrentBasicBlock != null)
                        {
                            Console.WriteLine("I'm in block " + currentBlockIndex.ToString("x"));

                            // Inherit state to outgoing edges.
                            if(outgoingEdges.TryGetValue(currentBlockIndex, out var destinations))
                            {
                                foreach(int destination in destinations)
                                {
                                    var destinationBlock = GetBasicBlock(destination);
                                    Console.WriteLine("  Inherit from " + currentBlockIndex.ToString("x") + " -> " + destination.ToString("x"));
                                    destinationBlock.InheritState(builder, CurrentBasicBlock.GetState());
                                }
                            }
                        }

                        // TODO: still not correct to always continue with next block

                        currentBlockIndex = insn.Offset;
                        CurrentBasicBlock = basicBlock;
                    }

                    CompileInstruction(insn, builder);

                    //Console.WriteLine("  stack count after insn: " + CurrentBasicBlock.GetState().StackSize);
                }
            }

            LLVM.DisposeBuilder(builder);
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
            compiler.InstructionProcessorDispatcher.Process(this, insn, builder);
        }
    }
}
