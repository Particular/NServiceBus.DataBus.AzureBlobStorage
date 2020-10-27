namespace NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests
{
    using System;
    using NUnit.Framework;
    
    public class When_validating_settings
    {
        [Test]
        public void Should_throw_when_no_configured_blobcontainerclient_nor_connectionstring()
        {
            var endpointConfiguration = new EndpointConfiguration("AzureBlobStorageDataBus.Test");
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.UseTransport<AcceptanceTestingTransport>();
            endpointConfiguration.UsePersistence<AcceptanceTestingPersistence>();
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.UseDataBus<AzureDataBus>();
       
            Assert.ThrowsAsync<Exception>(() => Endpoint.Start(endpointConfiguration));
        }
    }
}