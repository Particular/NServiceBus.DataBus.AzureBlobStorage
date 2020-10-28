namespace NServiceBus.DataBus.AzureBlobStorage.Config
{
    using Azure.Storage.Blobs;

    /// <summary>
    /// Provides a BlobContainerClient via dependency injection. A custom implementation can be registered on the container and will be picked up by the persistence.
    /// <remarks>     
    /// The client provided will not be disposed by the persistence. It is the responsibility of the provider to take care of proper resource disposal if necessary.
    /// </remarks>
    /// </summary>
    public interface IProvideBlobContainerClient
    {
        /// <summary>
        /// The BlobContainerClient to use
        /// </summary>
        BlobContainerClient Client { get; }
    }
}