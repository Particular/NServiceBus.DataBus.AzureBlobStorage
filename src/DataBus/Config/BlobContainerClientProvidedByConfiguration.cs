namespace NServiceBus.DataBus.AzureBlobStorage.Config
{
    using Azure.Storage.Blobs;
    class BlobContainerClientProvidedByConfiguration : IProvideBlobContainerClient
    {
        public BlobContainerClient Client { get; set; }
    }
}