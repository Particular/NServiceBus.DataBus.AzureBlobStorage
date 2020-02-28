using System;
using Microsoft.WindowsAzure.Storage.Auth;
using NServiceBus;
using NUnit.Framework;

public class When_configuring_token_provider_and_connection_string
{
    [Test]
    public void Should_throw()
    {
        var endpointConfiguration = new EndpointConfiguration("AzureBlobStorageDataBus.Test");
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.UseTransport<LearningTransport>();
        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.EnableInstallers();

        var dataBus = endpointConfiguration.UseDataBus<AzureDataBus>();
        dataBus.ConnectionString("user-provided-connection-string");
        dataBus.UseTokenCredentialForAccount(new TokenCredential(""), "user-provided-storage-account-name");

        Assert.ThrowsAsync<Exception>(() => Endpoint.Start(endpointConfiguration));
    }
}
