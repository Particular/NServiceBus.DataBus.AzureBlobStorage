using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests;

public class ConfigureDatabusEndpoint : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseDataBus<AzureDataBus>().Container(SetupFixture.ContainerName);

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        // Nothing required for in-memory persistence
        return Task.FromResult(0);
    }
}
