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
            fieldMap.Add(fieldDef, LLVM.AddGlobal(moduleRef, typeLookup.GetLLVMTypeRef(fieldDef.FieldType), fieldDef.FullName));
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
