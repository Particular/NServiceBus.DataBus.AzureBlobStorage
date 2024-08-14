using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.ClaimCheck.DataBus;
using NServiceBus.DataBus.AzureBlobStorage;
using NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests;

using SystemJsonDataBusSerializer = NServiceBus.ClaimCheck.DataBus.SystemJsonDataBusSerializer;

public class ConfigureDatabusEndpoint : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        configuration.UsePersistence<AcceptanceTestingPersistence>();
        configuration.UseClaimCheck<AzureDataBus, SystemJsonDataBusSerializer>().Container(SetupFixture.ContainerName);

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        // Nothing required for in-memory persistence
        return Task.FromResult(0);
    }
}
