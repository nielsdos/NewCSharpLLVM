using System.Collections.Generic;
using Mono.Cecil;
using LLVMSharp;

namespace CSharpLLVM
{
    /// <summary>
    /// Translates CIL types to LLVM types.
    /// </summary>
    public class TypeLookup
    {
        private Dictionary<MetadataType, LLVMTypeRef> typeMap = new Dictionary<MetadataType, LLVMTypeRef>();

        public TypeLookup()
        {
            // TODO
            typeMap.Add(MetadataType.Boolean, LLVM.Int1Type());
            typeMap.Add(MetadataType.SByte, LLVM.Int8Type());
            typeMap.Add(MetadataType.Byte, LLVM.Int8Type());
            typeMap.Add(MetadataType.Int16, LLVM.Int16Type());
            typeMap.Add(MetadataType.Int32, LLVM.Int32Type());
            typeMap.Add(MetadataType.Int64, LLVM.Int64Type());
            typeMap.Add(MetadataType.Single, LLVM.FloatType());
            typeMap.Add(MetadataType.Double, LLVM.DoubleType());
            typeMap.Add(MetadataType.Void, LLVM.VoidType());
        }

        /// <summary>
        /// Gets the LLVM type for the CIL type.
        /// </summary>
        /// <param name="typeRef">CIL type</param>
        /// <returns>LLVM Type</returns>
        public LLVMTypeRef GetLLVMTypeRef(TypeReference typeRef)
        {
            var mdt = typeRef.MetadataType;
            // TODO: support other things than primitives
            if(mdt == MetadataType.Class)
            {
                // TODO
                return LLVM.Int64Type();
            }
            return typeMap[mdt];
        }
    }
}
