namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using ApiApprover;
    using ApprovalTests.Reporters;
    using NUnit.Framework;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UseReporter(typeof(DiffReporter), typeof(AllFailingTestsClipboardReporter))]
        public void Approve()
        {
            var combine = Path.Combine(TestContext.CurrentContext.TestDirectory, "NServiceBus.DataBus.AzureBlobStorage.dll");
            var assembly = Assembly.LoadFile(combine);
            PublicApiApprover.ApprovePublicApi(assembly);
        }
    }
}
