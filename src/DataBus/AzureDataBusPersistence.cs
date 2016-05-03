namespace NServiceBus.DataBus.AzureBlobStorage
{
    using Microsoft.WindowsAzure.Storage;
    using NServiceBus;
    using Config;
    using Features;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
            Defaults(s =>
            {
                var configSection = s.GetConfigSection<AzureDataBusConfig>() ?? new AzureDataBusConfig();
                s.SetDefault("AzureDataBus.Container", configSection.Container);
                s.SetDefault("AzureDataBus.BasePath", configSection.BasePath);
                s.SetDefault("AzureDataBus.ConnectionString", configSection.ConnectionString);
                s.SetDefault("AzureDataBus.MaxRetries", configSection.MaxRetries);
                s.SetDefault("AzureDataBus.BackOffInterval", configSection.BackOffInterval);
                s.SetDefault("AzureDataBus.NumberOfIOThreads", configSection.NumberOfIOThreads);
                s.SetDefault("AzureDataBus.BlockSize", configSection.BlockSize);
                s.SetDefault("AzureDataBus.DefaultTTL", configSection.DefaultTTL);
            });
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var cloudBlobClient = CloudStorageAccount.Parse(context.Settings.Get<string>("AzureDataBus.ConnectionString")).CreateCloudBlobClient();

            var dataBus = new BlobStorageDataBus(cloudBlobClient.GetContainerReference(context.Settings.Get<string>("AzureDataBus.Container")))
            {
                BasePath = context.Settings.Get<string>("AzureDataBus.BasePath"),
                MaxRetries = context.Settings.Get<int>("AzureDataBus.MaxRetries"),
                BackOffInterval = context.Settings.Get<int>("AzureDataBus.BackOffInterval"),
                NumberOfIOThreads = context.Settings.Get<int>("AzureDataBus.NumberOfIOThreads"),
                BlockSize = context.Settings.Get<int>("AzureDataBus.BlockSize"),
                DefaultTTL = context.Settings.Get<long>("AzureDataBus.DefaultTTL")
            };

            context.Container.ConfigureComponent(b => dataBus, DependencyLifecycle.SingleInstance);
        }
    }
}