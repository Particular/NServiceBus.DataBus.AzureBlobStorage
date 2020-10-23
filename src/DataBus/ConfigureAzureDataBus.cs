using Azure.Storage.Blobs;

namespace NServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Configuration.AdvancedExtensibility;
    using DataBus;
    using DataBus.AzureBlobStorage;
    using Microsoft.Azure.Services.AppAuthentication;

    /// <summary>
    /// Configuration options for the Azure BlobStorage DataBus.
    /// </summary>
    public static partial class ConfigureAzureDataBus
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
        /// Sets the default time interval in milliseconds to perform periodic clean up of blobs for expired messages with specific TTL. Disabled by default.
        /// Note that value of zero (0) disables the periodic cleanup.
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
        /// Set the BlobContainerClient to use to control authentication against the service.
        /// Allows token-based, account shared key and connection string based authentication.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> UseBlobContainerClient(this DataBusExtensions<AzureDataBus> config,
            BlobContainerClient blobContainerClient)
        {
            Guard.AgainstNull(nameof(blobContainerClient), blobContainerClient);

            config.GetSettings().Set(SettingsKeys.BlobContainerClient, blobContainerClient);
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
    }
}
