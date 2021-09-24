namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using Features;
    using Config;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            if (!context.Container.HasComponent<IProvideBlobServiceClient>())
            {
                if (!context.Settings.TryGet(out IProvideBlobServiceClient blobContainerClientProvider) && dataBusSettings.ConnectionStringProvided)
                {
                    var blobContainerClient = CreateBlobServiceClient(dataBusSettings);
                    blobContainerClientProvider = new BlobServiceClientProvidedByConfiguration { Client = blobContainerClient };
                }

                var blobServiceClientProvider = blobContainerClientProvider ?? new ThrowIfNoBlobServiceClientProvider();
                context.Container.ConfigureComponent(() => blobServiceClientProvider, DependencyLifecycle.SingleInstance);
            }

            context.Container.ConfigureComponent<IDataBus>(serviceProvider => new BlobStorageDataBus(serviceProvider.Build<IProvideBlobServiceClient>(), dataBusSettings), DependencyLifecycle.SingleInstance);

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