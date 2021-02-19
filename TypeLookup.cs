using System.Collections.Generic;
using Mono.Cecil;
using LLVMSharp;

namespace CSharpLLVM
{
    /// <summary>
    /// Translates CIL types to LLVM types.
    /// </summary>
    public sealed class TypeLookup
    {
        private Dictionary<MetadataType, LLVMTypeRef> typeMap = new Dictionary<MetadataType, LLVMTypeRef>();
        private Dictionary<string, LLVMTypeRef> structureMap = new Dictionary<string, LLVMTypeRef>();
        private Dictionary<string, uint> indexMap = new Dictionary<string, uint>();

        public TypeLookup()
        {
            typeMap.Add(MetadataType.Boolean, LLVM.Int1Type());
            typeMap.Add(MetadataType.SByte, LLVM.Int8Type());
            typeMap.Add(MetadataType.Byte, LLVM.Int8Type());
            typeMap.Add(MetadataType.Int16, LLVM.Int16Type());
            typeMap.Add(MetadataType.Int32, LLVM.Int32Type());
            typeMap.Add(MetadataType.Int64, LLVM.Int64Type());
            typeMap.Add(MetadataType.Single, LLVM.FloatType());
            typeMap.Add(MetadataType.Double, LLVM.DoubleType());
            typeMap.Add(MetadataType.Void, LLVM.VoidType());
            typeMap.Add(MetadataType.Object, LLVM.PointerType(LLVM.VoidType(), 0));
        }

        /// <summary>
        /// Declares a type (without a body yet)
        /// </summary>
        /// <param name="typeDef">Type definition</param>
        public void DeclareType(TypeDefinition typeDef)
        {
            var structTypeRef = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), typeDef.FullName);
            structureMap.Add(typeDef.FullName, structTypeRef);
        }

        /// <summary>
        /// Defines a type that was previously declared (set the body)
        /// </summary>
        /// <param name="typeDef">Type definition</param>
        public void DefineType(TypeDefinition typeDef)
        {
            LLVMTypeRef[] typeRefs = new LLVMTypeRef[typeDef.Fields.Count];
            uint i = 0;
            foreach(FieldDefinition fieldDef in typeDef.Fields)
            {
                indexMap.Add(fieldDef.FullName, i);
                typeRefs[i++] = GetLLVMTypeRef(fieldDef.FieldType);
            }
            
            LLVM.StructSetBody(structureMap[typeDef.FullName], typeRefs, false);
        }

        /// <summary>
        /// Gets the index of a field in a structure.
        /// </summary>
        /// <param name="fieldRef">The field.</param>
        /// <returns>The index</returns>
        public uint GetIndexInStructure(FieldReference fieldRef)
        {
            return indexMap[fieldRef.FullName];
        }

        /// <summary>
        /// Gets the LLVM type for the CIL type.
        /// </summary>
        /// <param name="typeRef">CIL type</param>
        /// <returns>LLVM Type</returns>
        public LLVMTypeRef GetLLVMTypeRef(TypeReference typeRef)
        {
            var mdt = typeRef.MetadataType;
            if(mdt == MetadataType.Class)
            {
                return LLVM.PointerType(structureMap[typeRef.FullName], 0);
            }
            else if(mdt == MetadataType.ValueType)
            {
                return structureMap[typeRef.FullName];
            }
            return typeMap[mdt];
        }
    }
}
