#pragma warning disable CS0618 // Type or member is obsolete

namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using ClaimCheck.AzureBlobStorage;
    using Features;

    /// <summary>
    /// Provides a <see cref="BlobServiceClient"/> via dependency injection. A custom implementation can be registered on the container and will be picked up by the persistence.
    /// <remarks>
    /// The client provided will not be disposed by the persistence. It is the responsibility of the provider to take care of proper resource disposal if necessary.
    /// </remarks>
    /// </summary>
    [ObsoleteEx(Message = "NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient has been replaced by NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient.", RemoveInVersion = "8", TreatAsErrorFromVersion = "7", ReplacementTypeOrMember = "NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient")]
    public interface IProvideBlobServiceClient : NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient
    {
    }

    class AzureDataBusPersistence : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            throw new NotImplementedException();
        }
    }

    [ObsoleteEx]
    class BlobStorageDataBus : IDataBus, IDisposable
    {
        public BlobStorageDataBus(IProvideBlobServiceClient blobServiceClientProvider, ClaimCheckSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Get(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> Put(Stream stream, TimeSpan timeToBeReceived, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

namespace NServiceBus
{
    using System;
    using Azure.Storage.Blobs;
    using DataBus;

    /// <summary>
    /// DataBus implementation that uses azure blob storage.
    /// </summary>
    [ObsoleteEx(Message = "AzureDataBus has been replaced by AzureClaimCheck.", RemoveInVersion = "8", TreatAsErrorFromVersion = "7", ReplacementTypeOrMember = "AzureClaimCheck")]
    public class AzureDataBus : DataBusDefinition
    {
        /// <summary>The feature to enable when this databus is selected.</summary>
        protected override Type ProvidedByFeature() => throw new NotImplementedException();
    }

    /// <summary>
    /// Configuration options for the Azure BlobStorage DataBus.
    /// </summary>
    [ObsoleteEx(Message = "AzureDataBus has been replaced by AzureClaimCheck. These extension methods are replaced by the ones on the AzureClaimCheck type.", RemoveInVersion = "8", TreatAsErrorFromVersion = "7", ReplacementTypeOrMember = "AzureClaimCheck")]
    public static class ConfigureAzureDataBus
    {
        /// <summary>
        /// Sets the number of retries used by the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> MaxRetries(this DataBusExtensions<AzureDataBus> config, int maxRetries)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets backoff interval used by the blob storage client. Default is 30 seconds.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> BackOffInterval(this DataBusExtensions<AzureDataBus> config, int backOffInterval)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the number threads used the blob storage client. Default is 5.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> NumberOfIOThreads(this DataBusExtensions<AzureDataBus> config, int numberOfIOThreads)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The connection string to use. Default is `UseDevelopmentStorage=true`.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> ConnectionString(this DataBusExtensions<AzureDataBus> config, string connectionString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The blob container name to use. Default is ``.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> Container(this DataBusExtensions<AzureDataBus> config, string containerName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The base path within the container. Default is ``.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> BasePath(this DataBusExtensions<AzureDataBus> config, string basePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the custom <see cref="BlobServiceClient"/> to be used by the persistence, enabling the necessary customizations to the client.
        /// </summary>
        public static DataBusExtensions<AzureDataBus> UseBlobServiceClient(this DataBusExtensions<AzureDataBus> config,
            BlobServiceClient blobServiceClient)
        {
            throw new NotImplementedException();
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete