namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using DataBus;
    using DataBus.AzureBlobStorage;

    public static class ConfigureAzureDataBus
    {
        public static DataBusExtentions<AzureDataBus> MaxRetries(this DataBusExtentions<AzureDataBus> config, int maxRetries)
        {
            AzureDataBusGuard.CheckMaxRetries(maxRetries);

            GetSettings(config).MaxRetries = maxRetries;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> BackOffInterval(this DataBusExtentions<AzureDataBus> config, int backOffInterval)
        {
            AzureDataBusGuard.CheckBackOffInterval(backOffInterval);

            GetSettings(config).BackOffInterval = backOffInterval;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> BlockSize(this DataBusExtentions<AzureDataBus> config, int blockSize)
        {
            AzureDataBusGuard.CheckBlockSize(blockSize);

            GetSettings(config).BlockSize = blockSize;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> NumberOfIOThreads(this DataBusExtentions<AzureDataBus> config, int numberOfIOThreads)
        {
            AzureDataBusGuard.CheckNumberOfIOThreads(numberOfIOThreads);

            GetSettings(config).NumberOfIOThreads = numberOfIOThreads;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> ConnectionString(this DataBusExtentions<AzureDataBus> config, string connectionString)
        {
            AzureDataBusGuard.CheckConnectionString(connectionString);

            GetSettings(config).ConnectionString = connectionString;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> Container(this DataBusExtentions<AzureDataBus> config, string containerName)
        {
            AzureDataBusGuard.CheckContainerName(containerName);

            GetSettings(config).Container = containerName;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> BasePath(this DataBusExtentions<AzureDataBus> config, string basePath)
        {
            AzureDataBusGuard.CheckBasePath(basePath);

            GetSettings(config).BasePath = basePath;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> DefaultTTL(this DataBusExtentions<AzureDataBus> config, long defaultTTLInSeconds)
        {
            AzureDataBusGuard.CheckDefaultTTL(defaultTTLInSeconds);

            GetSettings(config).TTL = defaultTTLInSeconds;
            return config;
        }

        static DataBusSettings GetSettings(DataBusExtentions<AzureDataBus> config)
        {
            DataBusSettings settings;
            if (!config.GetSettings().TryGet(out settings))
            {
                settings = new DataBusSettings();
                config.GetSettings().Set<DataBusSettings>(settings);
            }

            return settings;
        }
    }
}