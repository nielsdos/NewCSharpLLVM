using System.Collections.Generic;
using Mono.Cecil;
using LLVMSharp;

namespace CSharpLLVM
{
    public sealed class StaticFieldLookup
    {
        private Dictionary<FieldReference, LLVMValueRef> fieldMap = new Dictionary<FieldReference, LLVMValueRef>();
        private TypeLookup typeLookup;
        private LLVMModuleRef moduleRef;

        public StaticFieldLookup(LLVMModuleRef moduleRef, TypeLookup typeLookup)
        {
            this.moduleRef = moduleRef;
            this.typeLookup = typeLookup;
        }

        /// <summary>
        /// Declares a static field.
        /// </summary>
        /// <param name="fieldDef">The static field definition</param>
        public void DeclareField(FieldDefinition fieldDef)
        {
            var globalType = typeLookup.GetLLVMTypeRef(fieldDef.FieldType);
            var globalValue = LLVM.AddGlobal(moduleRef, globalType, fieldDef.FullName);
            fieldMap.Add(fieldDef, globalValue);

            // TODO: optimize using InitialValue?
            /*System.Console.WriteLine(fieldDef.InitialValue);
            foreach(byte b in fieldDef.InitialValue)
                System.Console.WriteLine(b);*/
            
            LLVM.SetInitializer(globalValue, LLVM.ConstNull(globalType));
        }

        /// <summary>
        /// Declare all static fields for a type definition.
        /// </summary>
        /// <param name="typeDef">The type definition</param>
        public void DeclareFieldsFor(TypeDefinition typeDef)
        {
            foreach(var fieldDef in typeDef.Fields)
            {
                DeclareField(fieldDef);
            }
        }

        /// <summary>
        /// Gets the LLVM value for a field reference.
        /// </summary>
        /// <param name="fieldRef">The field reference</param>
        /// <returns>The LLVM value</returns>
        public LLVMValueRef GetField(FieldReference fieldRef)
        {
            return fieldMap[fieldRef];
        }
    }
}
