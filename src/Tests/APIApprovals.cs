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
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(AzureDataBus).Assembly);
            Approver.Verify(publicApi);
        }
    }
}
