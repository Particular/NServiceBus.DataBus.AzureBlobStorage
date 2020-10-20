using System;
using NServiceBus;
using NUnit.Framework;

public class When_authenticating_with_managed_identity
{
    [Test]
    public void Should_throw_if_connection_string_is_also_provided()
    {
        var endpointConfiguration = new EndpointConfiguration("AzureBlobStorageDataBus.Test");
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.UseTransport<AcceptanceTestingTransport>();
        endpointConfiguration.UsePersistence<AcceptanceTestingPersistence>();
        endpointConfiguration.EnableInstallers();

        var dataBus = endpointConfiguration.UseDataBus<AzureDataBus>();
        dataBus.ConnectionString("user-provided-connection-string");
        dataBus.AuthenticateWithManagedIdentity("user-provided-storage-account-name", TimeSpan.FromMinutes(5));

        Assert.ThrowsAsync<Exception>(() => Endpoint.Start(endpointConfiguration));
    }
}
