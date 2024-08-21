namespace NServiceBus.DataBus.AzureBlobStorage
{
    using Azure.Core;
    using Azure.Storage.Blobs;
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.ClaimCheck.AzureBlobStorage;
    using NServiceBus.ClaimCheck.AzureBlobStorage.Config;
    using NServiceBus.Features;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            DependsOn<DataBus>();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var claimCheckSettings = context.Settings.GetOrDefault<ClaimCheckSettings>() ?? new ClaimCheckSettings();

            // If a service client has been registered in the container, it will added later in the configuration process and replace any client set here
            if (!context.Settings.TryGet(out IProvideBlobServiceClient blobContainerClientProvider) && claimCheckSettings.ConnectionStringProvided)
            {
                var blobContainerClient = CreateBlobServiceClient(claimCheckSettings);
                blobContainerClientProvider = new BlobServiceClientProvidedByConfiguration { Client = blobContainerClient };
            }

            context.Services.AddSingleton(blobContainerClientProvider ?? new ThrowIfNoBlobServiceClientProvider());
#pragma warning disable CS0618 // Type or member is obsolete
            context.Services.AddSingleton<IDataBus>(serviceProvider => new BlobStorageDataBus(serviceProvider.GetRequiredService<IProvideBlobServiceClient>(),
                claimCheckSettings));
#pragma warning restore CS0618 // Type or member is obsolete

            context.Settings.AddStartupDiagnosticsSection(
                typeof(AzureClaimCheck).FullName,
                new
                {
                    ConnectionMechanism = claimCheckSettings.ConnectionStringProvided ? "ConnectionString" : "BlobServiceClient",
                    ContainerName = claimCheckSettings.Container,
                    claimCheckSettings.MaxRetries,
                    claimCheckSettings.BackOffInterval,
                    claimCheckSettings.NumberOfIOThreads,
                });
        }

        BlobServiceClient CreateBlobServiceClient(ClaimCheckSettings claimCheckSettings)
        {
            var clientOptions = new BlobClientOptions
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(claimCheckSettings.BackOffInterval),
                    MaxRetries = claimCheckSettings.MaxRetries,
                    Mode = RetryMode.Fixed
                }
            };
            return new BlobServiceClient(claimCheckSettings.ConnectionString, clientOptions);
        }
    }
}
