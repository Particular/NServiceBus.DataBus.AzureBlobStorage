namespace NServiceBus.ClaimCheck.AzureBlobStorage
{
    using Azure.Storage.Blobs;

    /// <summary>
    /// Provides a <see cref="BlobServiceClient"/> via dependency injection. A custom implementation can be registered on the container and will be picked up by the persistence.
    /// <remarks>
    /// The client provided will not be disposed by the persistence. It is the responsibility of the provider to take care of proper resource disposal if necessary.
    /// </remarks>
    /// </summary>
    public interface IProvideBlobServiceClient : DataBus.AzureBlobStorage.IProvideBlobServiceClient
    {
    }
}

namespace NServiceBus.DataBus.AzureBlobStorage
{
    using Azure.Storage.Blobs;

    /// <summary>
    /// Provides a <see cref="BlobServiceClient"/> via dependency injection. A custom implementation can be registered on the container and will be picked up by the persistence.
    /// <remarks>
    /// The client provided will not be disposed by the persistence. It is the responsibility of the provider to take care of proper resource disposal if necessary.
    /// </remarks>
    /// </summary>
    [ObsoleteEx(Message = "NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient has been replaced by NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient.", RemoveInVersion = "8", TreatAsErrorFromVersion = "7", ReplacementTypeOrMember = "NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient")]
    public interface IProvideBlobServiceClient
    {
        /// <summary>
        /// The BlobServiceClient to use
        /// </summary>
        BlobServiceClient Client { get; }
    }
}