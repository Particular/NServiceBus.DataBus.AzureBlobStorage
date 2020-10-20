namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using Features;

    internal class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            var blobContainerClientConfiguredByUser = context.Settings.TryGet(out BlobContainerClient blobContainerClient);
            ThrowIfMissingConfigurationForBlobContainer(blobContainerClientConfiguredByUser, dataBusSettings);
            if (!blobContainerClientConfiguredByUser)
            {
                blobContainerClient = CreateBlobContainerClient(dataBusSettings);
            }

            var dataBus = new BlobStorageDataBus(blobContainerClient, dataBusSettings, new AsyncTimer());
            context.Services.AddSingleton<IDataBus>(b => dataBus);
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

            if (string.IsNullOrWhiteSpace(dataBusSettings.ConnectionString) ||
                string.IsNullOrWhiteSpace(dataBusSettings.Container))
            {
                throw new Exception("Unable to find a configured BlobClient in the container and unable to fall back to a connectionstring + container as none were supplied.");
            }
        }
    }
}