namespace NServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Configuration.AdvancedExtensibility;
    using DataBus;
    using DataBus.AzureBlobStorage;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.WindowsAzure.Storage.Auth;

    /// <summary>
    /// Configuration options for the Azure BlobStorage DataBus.
    /// </summary>
    public static class ConfigureAzureDataBus
    {
        /// <summary>
        /// Sets the number of retries used by the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> MaxRetries(this DataBusExtensions<AzureDataBus> config, int maxRetries)
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
        public static DataBusExtensions<AzureDataBus> BackOffInterval(this DataBusExtensions<AzureDataBus> config, int backOffInterval)
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
        public static DataBusExtensions<AzureDataBus> BlockSize(this DataBusExtensions<AzureDataBus> config, int blockSize)
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
        public static DataBusExtensions<AzureDataBus> NumberOfIOThreads(this DataBusExtensions<AzureDataBus> config, int numberOfIOThreads)
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
        public static DataBusExtensions<AzureDataBus> ConnectionString(this DataBusExtensions<AzureDataBus> config, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Should not be an empty string.", nameof(connectionString));
            }

            var dataBusSettings = GetSettings(config);

            dataBusSettings.ConnectionString = connectionString;
            dataBusSettings.UserProvidedConnectionString = true;

            return config;
        }

        /// <summary>
        /// The blob container name to use. Default is ``.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> Container(this DataBusExtensions<AzureDataBus> config, string containerName)
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
        public static DataBusExtensions<AzureDataBus> BasePath(this DataBusExtensions<AzureDataBus> config, string basePath)
        {
            var value = basePath ?? " ";
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
        public static DataBusExtensions<AzureDataBus> DefaultTTL(this DataBusExtensions<AzureDataBus> config, long defaultTTLInSeconds)
        {
            if (defaultTTLInSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultTTLInSeconds), defaultTTLInSeconds, "Should not be negative.");
            }

            GetSettings(config).TTL = defaultTTLInSeconds;
            return config;
        }

        /// <summary>
        /// Sets the default time interval to perform periodic clean up of blobs for expired messages with specific TTL. Default is 5 minutes.
        /// Note that value of zero (0) will disable periodic cleanup.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> CleanupInterval(this DataBusExtensions<AzureDataBus> config, int cleanupInterval)
        {
            if (cleanupInterval < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cleanupInterval), cleanupInterval, "Should not be negative.");
            }

            GetSettings(config).CleanupInterval = cleanupInterval;
            return config;
        }

        /// <summary>
        ///  Sets token credential to authenticate with Storage Blob service
        /// <remarks>Token credentials can be created using <see cref="AzureServiceTokenProvider"/> with token renewal configured.</remarks>
        /// </summary>
        public static DataBusExtensions<AzureDataBus> AuthenticateWithManagedIdentity(this DataBusExtensions<AzureDataBus> config, string storageAccountName, TimeSpan renewalTimeBeforeTokenExpires)
        {
            if (renewalTimeBeforeTokenExpires <= TimeSpan.Zero)
            {
                throw new ArgumentException($"Should not be less or equal to {nameof(TimeSpan.Zero)}", nameof(renewalTimeBeforeTokenExpires));
            }

            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                throw new ArgumentException("Should not be null or empty", nameof(storageAccountName));
            }

            var dataBusSettings = GetSettings(config);

            dataBusSettings.RenewalTimeBeforeTokenExpires = renewalTimeBeforeTokenExpires;
            dataBusSettings.StorageAccountName = storageAccountName;

            return config;
        }

        static bool IsValidBlobContainerName(object containerName)
        {
            return !string.IsNullOrWhiteSpace((string)containerName) &&
                   Regex.IsMatch((string)containerName, @"^(([a-z\d]((-(?=[a-z\d]))|([a-z\d])){2,62})|(\$root))$");
        }

        static DataBusSettings GetSettings(DataBusExtensions<AzureDataBus> config)
        {
            if (!config.GetSettings().TryGet<DataBusSettings>(out var settings))
            {
                settings = new DataBusSettings();
                config.GetSettings().Set<DataBusSettings>(settings);
            }

            return settings;
        }

        internal const int MaxBlockSize = 4 * 1024 * 1024; //4 mb
    }
}