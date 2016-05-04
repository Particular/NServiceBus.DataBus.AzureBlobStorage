namespace NServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Configuration.AdvanceExtensibility;
    using DataBus;
    using DataBus.AzureBlobStorage;

    public static class ConfigureAzureDataBus
    {
        public static DataBusExtentions<AzureDataBus> MaxRetries(this DataBusExtentions<AzureDataBus> config, int maxRetries)
        {
            if (maxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries), maxRetries, "Must be non negative.");
            }

            GetSettings(config).MaxRetries = maxRetries;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> BackOffInterval(this DataBusExtentions<AzureDataBus> config, int backOffInterval)
        {
            if (backOffInterval < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(backOffInterval), backOffInterval, "Must not be negative.");
            }

            GetSettings(config).BackOffInterval = backOffInterval;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> BlockSize(this DataBusExtentions<AzureDataBus> config, int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Must not be negative.");
            }

            if (blockSize > MaxBlockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Must be less than 4mb");
            }

            GetSettings(config).BlockSize = blockSize;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> NumberOfIOThreads(this DataBusExtentions<AzureDataBus> config, int numberOfIOThreads)
        {
            if (numberOfIOThreads <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfIOThreads), numberOfIOThreads, "Should not be less than one.");
            }

            GetSettings(config).NumberOfIOThreads = numberOfIOThreads;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> ConnectionString(this DataBusExtentions<AzureDataBus> config, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Should not be an empty string.", nameof(connectionString));
            }

            GetSettings(config).ConnectionString = connectionString;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> Container(this DataBusExtentions<AzureDataBus> config, string containerName)
        {
          
            if (!IsValidBlobContainerName(containerName))
            {
                const string errorMessage =
                "Invalid container name. The container name must be confirming to the following naming rules:" +
                "1. Must start with a letter or number, and can contain only letters, numbers, and the dash (-) character." +
                "2. Every dash (-) character must be immediately preceded and followed by a letter or number." +
                "3. All letters must be lowercase." +
                "4. Container name must be from 3 through 63 characters long.";

                throw new ArgumentException(errorMessage, nameof(containerName));
            }

           
            GetSettings(config).Container = containerName;
            return config;
        }
        
        public static DataBusExtentions<AzureDataBus> BasePath(this DataBusExtentions<AzureDataBus> config, string basePath)
        {
            var value = basePath != null ? basePath : " ";
            var spacesOnly = value.Trim().Length == 0 && value.Length != 0;

            if (spacesOnly)
            {
                throw new ArgumentException("Should not be null or spaces only.", nameof(basePath));
            }

            GetSettings(config).BasePath = basePath;
            return config;
        }

        public static DataBusExtentions<AzureDataBus> DefaultTTL(this DataBusExtentions<AzureDataBus> config, long defaultTTLInSeconds)
        {
            if (defaultTTLInSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultTTLInSeconds), defaultTTLInSeconds, "Should not be negative.");
            }
            GetSettings(config).TTL = defaultTTLInSeconds;
            return config;
        }

        static bool IsValidBlobContainerName(object containerName)
        {
            return !string.IsNullOrWhiteSpace((string)containerName) &&
                   Regex.IsMatch((string)containerName, @"^(([a-z\d]((-(?=[a-z\d]))|([a-z\d])){2,62})|(\$root))$");
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

        internal const int MaxBlockSize = 4 * 1024 * 1024; //4 mb
    }
}