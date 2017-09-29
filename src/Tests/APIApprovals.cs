#if NET452
namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using ApprovalTests;
    using NUnit.Framework;
    using PublicApiGenerator;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Approve()
        {
            var combine = Path.Combine(TestContext.CurrentContext.TestDirectory, "NServiceBus.DataBus.AzureBlobStorage.dll");
            var assembly = Assembly.LoadFile(combine);
            var publicApi = ApiGenerator.GeneratePublicApi(assembly);
            Approvals.Verify(publicApi);
        }
    }
}
#endif