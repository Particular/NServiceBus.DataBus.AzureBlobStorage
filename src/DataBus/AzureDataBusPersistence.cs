namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using Features;
    using System.Linq;
    using Config;
    
    internal class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            // If a provider has been registered in the container, it will added later in the configuration process and replace any client set here
            var blobContainerClientConfiguredInSettings = context.Settings.TryGet(out IProvideBlobContainerClient blobContainerClientProvider);
            if (blobContainerClientConfiguredInSettings)
            {
                context.Services.AddSingleton(b => blobContainerClientProvider);
            }
            else if (dataBusSettings.UserProvidedConnectionString)
            {
                var blobContainerClient = CreateBlobContainerClient(dataBusSettings);
                blobContainerClientProvider = new BlobContainerClientProvidedByConfiguration
                    {Client = blobContainerClient};
                context.Services.AddSingleton(b => blobContainerClientProvider);
            }
            
            context.Services.AddSingleton<IDataBus>(serviceProvider => new BlobStorageDataBus(serviceProvider.GetRequiredService<IProvideBlobContainerClient>(),
                dataBusSettings, new AsyncTimer()));
        }

        private BlobContainerClient CreateBlobContainerClient(DataBusSettings dataBusSettings)
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
            return new BlobContainerClient(dataBusSettings.ConnectionString, dataBusSettings.Container, clientOptions);
        }
    }
}