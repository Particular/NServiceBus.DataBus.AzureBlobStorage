namespace NServiceBus.ClaimCheck.AzureBlobStorage
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using Features;
    using NServiceBus.ClaimCheck;
    using NServiceBus.ClaimCheck.AzureBlobStorage.Config;

    class AzureClaimCheckPersistence : Feature
    {
        public AzureClaimCheckPersistence()
        {
            DependsOn<ClaimCheckFeature>();
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
            context.Services.AddSingleton(serviceProvider => new BlobStorageClaimCheck(serviceProvider.GetRequiredService<IProvideBlobServiceClient>(), claimCheckSettings));
            context.Services.AddTransient<IClaimCheck>(serviceProvider => serviceProvider.GetService<BlobStorageClaimCheck>());

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