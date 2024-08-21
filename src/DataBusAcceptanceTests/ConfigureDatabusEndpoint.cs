using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.ClaimCheck;
using NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests;

public class ConfigureDatabusEndpoint : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        configuration.UsePersistence<AcceptanceTestingPersistence>();
#pragma warning disable CS0618 // Type or member is obsolete
        configuration.UseDataBus<AzureDataBus, SystemJsonDataBusSerializer>().Container(SetupFixture.ContainerName);
#pragma warning restore CS0618 // Type or member is obsolete

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        // Nothing required for in-memory persistence
        return Task.FromResult(0);
    }
}
