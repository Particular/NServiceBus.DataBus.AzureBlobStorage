namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Features;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            ThrowIfConnectionStringAndTokenProviderSpecified(dataBusSettings);

            CloudBlobContainer container;

            // Attempt managed identity identity first
            if (dataBusSettings.TokenCredential != null)
            {
                var storageCredentials = new StorageCredentials(dataBusSettings.TokenCredential);
                var containerPath = $"https://{dataBusSettings.StorageAccountName}.blob.core.windows.net/{dataBusSettings.Container}";
                container = new CloudBlobContainer(new StorageUri(new Uri(containerPath)), storageCredentials);
            }
            else // fallback to connection string
            {
                var cloudBlobClient = CloudStorageAccount.Parse(dataBusSettings.ConnectionString).CreateCloudBlobClient();
                container = cloudBlobClient.GetContainerReference(dataBusSettings.Container);
            }

            var dataBus = new BlobStorageDataBus(container, dataBusSettings, new AsyncTimer());

            context.Container.ConfigureComponent(b => dataBus, DependencyLifecycle.SingleInstance);
        }

        static void ThrowIfConnectionStringAndTokenProviderSpecified(DataBusSettings dataBusSettings)
        {
            if (dataBusSettings.TokenCredential != null && dataBusSettings.UserProvidedConnectionString)
            {
                throw new Exception("More than one method to connect to the storage account was supplied (using connection string and token provider). Use one method only.");
            }
        }
    }
}