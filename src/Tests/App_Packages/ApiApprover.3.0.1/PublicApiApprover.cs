namespace ApiApprover
{
    using System.IO;
    using ApprovalTests;
    using ApprovalTests.Namers;
    using Mono.Cecil;

    public static class PublicApiApprover
    {
        public static void ApprovePublicApi(string assemblyPath)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));

            var readSymbols = File.Exists(Path.ChangeExtension(assemblyPath, ".pdb"));
            var asm = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters(ReadingMode.Deferred)
            {
                ReadSymbols = readSymbols,
                AssemblyResolver = assemblyResolver
            });

            var publicApi = PublicApiGenerator.CreatePublicApiForAssembly(asm);
            var writer = new ApprovalTextWriter(publicApi, "cs");
            var approvalNamer = new AssemblyPathNamer(assemblyPath);
            Approvals.Verify(writer, approvalNamer, Approvals.GetReporter());
        }

        private class AssemblyPathNamer : UnitTestFrameworkNamer
        {
            public AssemblyPathNamer(string assemblyPath)
            {
                Name = Path.GetFileNameWithoutExtension(assemblyPath);
            }

            public override string Name { get; }
        }
    }
}