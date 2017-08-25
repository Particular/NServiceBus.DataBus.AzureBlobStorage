namespace NServiceBus.DataBus.AzureBlobStorage
{
    using Features;
    using Microsoft.WindowsAzure.Storage;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            var cloudBlobClient = CloudStorageAccount.Parse(dataBusSettings.ConnectionString).CreateCloudBlobClient();

            var dataBus = new BlobStorageDataBus(cloudBlobClient.GetContainerReference(dataBusSettings.Container), dataBusSettings, new AsyncTimer());

            context.Container.ConfigureComponent(b => dataBus, DependencyLifecycle.SingleInstance);
        }
    }
}