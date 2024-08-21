namespace NServiceBus.ClaimCheck.AzureBlobStorage.Config
{
    using Azure.Storage.Blobs;
    class BlobServiceClientProvidedByConfiguration : IProvideBlobServiceClient
    {
        public BlobServiceClient Client { get; set; }
    }
}