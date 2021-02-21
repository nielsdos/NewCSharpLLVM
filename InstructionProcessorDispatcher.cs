using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using LLVMSharp;

namespace CSharpLLVM
{
    public class InstructionProcessorDispatcher
    {
        private Dictionary<Code, InstructionProcessor> processors = new Dictionary<Code, InstructionProcessor>();

        public InstructionProcessorDispatcher()
        {
            // Load code processors.
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                // Check and get handler.
                InstructionHandlerAttribute attrib = (InstructionHandlerAttribute) type.GetCustomAttribute(typeof(InstructionHandlerAttribute));
                if (attrib == null)
                    continue;

                // Register emitter.
                var emitter = (InstructionProcessor) Activator.CreateInstance(type);
                foreach (Code code in attrib.Codes)
                    processors.Add(code, emitter);
            }
        }

        /// <summary>
        /// Processes an instruction.
        /// </summary>
        /// <param name="compiler">The method compiler.</param>
        /// <param name="insn">The instruction.</param>
        /// <param name="builder">The builder.</param>
        public void Process(MethodCompiler compiler, Instruction insn, LLVMBuilderRef builder)
        {
            if (processors.TryGetValue(insn.OpCode.Code, out var processor))
            {
                processor.Process(compiler, insn, builder);
            }
            else
            {
                //throw new NotImplementedException("Instruction with opcode " + insn.OpCode.Code + " is not implemented");
                // TODO
                Console.WriteLine("unimplemented " + insn.OpCode.Code);
            }
        }
    }
}
