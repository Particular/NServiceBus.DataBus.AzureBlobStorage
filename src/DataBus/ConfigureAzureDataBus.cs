namespace NServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Configuration.AdvanceExtensibility;
    using DataBus;
    using DataBus.AzureBlobStorage;

    /// <summary>
    /// Configuration options for the Azure BlobStorage DataBus.
    /// </summary>
    public static class ConfigureAzureDataBus
    {
        /// <summary>
        /// Sets the number of retries used by the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtentions<AzureDataBus> MaxRetries(this DataBusExtentions<AzureDataBus> config, int maxRetries)
        {
            if (maxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries), maxRetries, "Must be non negative.");
            }

            GetSettings(config).MaxRetries = maxRetries;
            return config;
        }

        /// <summary>
        /// Sets backoff intervall used by the blob storage client. Default is 30 seconds.
        /// </summary>
        public static DataBusExtentions<AzureDataBus> BackOffInterval(this DataBusExtentions<AzureDataBus> config, int backOffInterval)
        {
            if (backOffInterval < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(backOffInterval), backOffInterval, "Must not be negative.");
            }

            GetSettings(config).BackOffInterval = backOffInterval;
            return config;
        }

        /// <summary>
        /// Sets the block size used by the blob storage client. Default is 4mb which also is the maximum for blob storage.
        /// </summary>
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

        /// <summary>
        /// Sets the number threads used the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtentions<AzureDataBus> NumberOfIOThreads(this DataBusExtentions<AzureDataBus> config, int numberOfIOThreads)
        {
            if (numberOfIOThreads <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfIOThreads), numberOfIOThreads, "Should not be less than one.");
            }

            GetSettings(config).NumberOfIOThreads = numberOfIOThreads;
            return config;
        }

        /// <summary>
        /// The connection string to use. Default is `UseDevelopmentStorage=true`.
        /// </summary>
        public static DataBusExtentions<AzureDataBus> ConnectionString(this DataBusExtentions<AzureDataBus> config, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Should not be an empty string.", nameof(connectionString));
            }

            GetSettings(config).ConnectionString = connectionString;
            return config;
        }

        /// <summary>
        /// The blob container name to use. Default is ``.
        /// </summary>
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

        /// <summary>
        /// The base path within the container. Default is ``.
        /// </summary>
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

        /// <summary>
        /// Sets the default TTL to use for messages with no specific TTL. By default no TTL is set and messages are kept forever.
        /// Note that messages in flight or in the error queue can no longer be processed when DataBus entry has been removed.
        /// </summary>
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