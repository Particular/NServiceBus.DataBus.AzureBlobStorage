namespace NServiceBus.DataBus.AzureBlobStorage.Config
{
    using Azure.Storage.Blobs;
    class BlobServiceClientProvidedByConfiguration : IProvideBlobServiceClient
    {
        public BlobServiceClient Client { get; set; }
    }
}