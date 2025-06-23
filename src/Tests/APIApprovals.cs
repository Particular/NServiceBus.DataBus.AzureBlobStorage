namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using NUnit.Framework;
    using Particular.Approvals;
    using PublicApiGenerator;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        public void Approve()
        {
            var publicApi = typeof(AzureClaimCheck).Assembly.GeneratePublicApi(new ApiGeneratorOptions
            {
                ExcludeAttributes = ["System.Runtime.Versioning.TargetFrameworkAttribute", "System.Reflection.AssemblyMetadataAttribute"]
            });
            Approver.Verify(publicApi);
        }
    }
}