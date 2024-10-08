﻿using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.ClaimCheck.AzureBlobStorage.AcceptanceTests;

public class ConfigureClaimCheckEndpoint : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        configuration.UsePersistence<AcceptanceTestingPersistence>();
        configuration.UseClaimCheck<AzureClaimCheck, SystemJsonClaimCheckSerializer>()
            .Container(SetupFixture.ContainerName);

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        // Nothing required for in-memory persistence
        return Task.FromResult(0);
    }
}
