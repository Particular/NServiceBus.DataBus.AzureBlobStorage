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

            // If no provider has been registered in the container, look for on in the settings
            if (context.Services.All(x => x.ServiceType != typeof(IProvideBlobContainerClient)))
            {
                var blobContainerClientConfiguredByUser = context.Settings.TryGet(out IProvideBlobContainerClient blobContainerClientProvider);
                ThrowIfMissingConfigurationForBlobContainer(blobContainerClientConfiguredByUser, dataBusSettings);
                if (!blobContainerClientConfiguredByUser)
                {
                    var blobContainerClient = CreateBlobContainerClient(dataBusSettings);
                    blobContainerClientProvider = new BlobContainerClientProvidedByConfiguration{Client = blobContainerClient};
                }
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

        private void ThrowIfMissingConfigurationForBlobContainer(bool isBlobClientConfiguredByUser,
            DataBusSettings dataBusSettings)
        {
            if (isBlobClientConfiguredByUser)
            {
                return;
            }

            if (!dataBusSettings.UserProvidedConnectionString)
            {
                throw new Exception("Azure databus was not configured to use a BlobContainerClient. Use '.UseBlobContainerClient()' to provide a BlobContainerClient to be used. Alternatively, configure the data bus using a connection string and a container name.");
            }
        }
    }
}