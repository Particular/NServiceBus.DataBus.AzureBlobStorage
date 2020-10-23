namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
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

            var blobContainerClientConfiguredByUser = context.Settings.TryGet(SettingsKeys.BlobContainerClient, out BlobContainerClient blobContainerClient);
            ThrowIfMissingConfigurationForBlobContainer(blobContainerClientConfiguredByUser, dataBusSettings);
            if (!blobContainerClientConfiguredByUser)
            {
                blobContainerClient = CreateBlobContainerClient(dataBusSettings);
            }

            var dataBus = new BlobStorageDataBus(blobContainerClient, dataBusSettings, new AsyncTimer());
            context.Container.RegisterSingleton(dataBus);
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