namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using Features;
    using ClaimCheck;
    using Config;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<ClaimCheckFeature>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            // If a service client has been registered in the container, it will added later in the configuration process and replace any client set here
            if (!context.Settings.TryGet(out IProvideBlobServiceClient blobContainerClientProvider) && dataBusSettings.ConnectionStringProvided)
            {
                var blobContainerClient = CreateBlobServiceClient(dataBusSettings);
                blobContainerClientProvider = new BlobServiceClientProvidedByConfiguration { Client = blobContainerClient };
            }

            context.Services.AddSingleton(blobContainerClientProvider ?? new ThrowIfNoBlobServiceClientProvider());
            context.Services.AddSingleton<IClaimCheck>(serviceProvider => new BlobStorageDataBus(serviceProvider.GetRequiredService<IProvideBlobServiceClient>(),
                dataBusSettings));

            context.Settings.AddStartupDiagnosticsSection(
                typeof(AzureDataBus).FullName,
                new
                {
                    ConnectionMechanism = dataBusSettings.ConnectionStringProvided ? "ConnectionString" : "BlobServiceClient",
                    ContainerName = dataBusSettings.Container,
                    dataBusSettings.MaxRetries,
                    dataBusSettings.BackOffInterval,
                    dataBusSettings.NumberOfIOThreads,
                });
        }

        BlobServiceClient CreateBlobServiceClient(DataBusSettings dataBusSettings)
        {
            var clientOptions = new BlobClientOptions
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(dataBusSettings.BackOffInterval),
                    MaxRetries = dataBusSettings.MaxRetries,
                    Mode = RetryMode.Fixed
                }
            };
            return new BlobServiceClient(dataBusSettings.ConnectionString, clientOptions);
        }
    }
}