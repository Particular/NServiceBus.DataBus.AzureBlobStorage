﻿namespace NServiceBus
{
    using System;
    using System.Text.RegularExpressions;
    using Azure.Storage.Blobs;
    using Configuration.AdvancedExtensibility;
    using DataBus;
    using NServiceBus.ClaimCheck.AzureBlobStorage;
    using NServiceBus.ClaimCheck.AzureBlobStorage.Config;
    using IProvideBlobServiceClient = ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient;

    /// <summary>
    /// Configuration options for the Azure BlobStorage DataBus.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    [ObsoleteEx(Message = "AzureDataBus has been replaced by AzureClaimCheck. These extension methods are replaced by the ones on the AzureClaimCheck type.", RemoveInVersion = "8", TreatAsErrorFromVersion = "7", ReplacementTypeOrMember = "AzureClaimCheck")]
    public static class ConfigureAzureDataBus
    {
        /// <summary>
        /// Sets the number of retries used by the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> MaxRetries(this DataBusExtensions<AzureDataBus> config, int maxRetries)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);

            GetSettings(config).MaxRetries = maxRetries;
            return config;
        }

        /// <summary>
        /// Sets backoff interval used by the blob storage client. Default is 30 seconds.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> BackOffInterval(this DataBusExtensions<AzureDataBus> config, int backOffInterval)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(backOffInterval);

            GetSettings(config).BackOffInterval = backOffInterval;
            return config;
        }

        /// <summary>
        /// Sets the number threads used the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> NumberOfIOThreads(this DataBusExtensions<AzureDataBus> config, int numberOfIOThreads)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numberOfIOThreads);

            GetSettings(config).NumberOfIOThreads = numberOfIOThreads;
            return config;
        }

        /// <summary>
        /// The connection string to use. Default is `UseDevelopmentStorage=true`.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> ConnectionString(this DataBusExtensions<AzureDataBus> config, string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            var dataBusSettings = GetSettings(config);

            dataBusSettings.ConnectionString = connectionString;
            dataBusSettings.ConnectionStringProvided = true;

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
        /// Set the custom <see cref="BlobServiceClient"/> to be used by the persistence, enabling the necessary customizations to the client.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> UseBlobServiceClient(this DataBusExtensions<AzureDataBus> config,
            BlobServiceClient blobServiceClient)
        {
            ArgumentNullException.ThrowIfNull(blobServiceClient);

            config.GetSettings().Set<IProvideBlobServiceClient>(new BlobServiceClientProvidedByConfiguration { Client = blobServiceClient });
            return config;
        }

        static bool IsValidBlobContainerName(object containerName)
        {
            return !string.IsNullOrWhiteSpace((string)containerName) &&
                   Regex.IsMatch((string)containerName, @"^(([a-z\d]((-(?=[a-z\d]))|([a-z\d])){2,62})|(\$root))$");
        }

        static ClaimCheckSettings GetSettings(DataBusExtensions<AzureDataBus> config)
        {
            if (!config.GetSettings().TryGet<ClaimCheckSettings>(out var settings))
            {
                settings = new ClaimCheckSettings();
                config.GetSettings().Set(settings);
            }

            return settings;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
