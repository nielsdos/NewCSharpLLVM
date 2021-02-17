using Mono.Cecil;

namespace CSharpLLVM
{
    class Program
    {
        static void Main(string[] args)
        {
            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly("/home/niels/csharp_tests/bin/Debug/netstandard2.0/csharp_tests.dll");
            Compiler compiler = new Compiler(asmDef);
            compiler.Compile();
        }
    }
}
