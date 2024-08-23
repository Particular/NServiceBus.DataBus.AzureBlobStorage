namespace NServiceBus.ClaimCheck.AzureBlobStorage.Config
{
    using Azure.Storage.Blobs;
    class BlobServiceClientProvidedByConfiguration : NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient
    {
        public BlobServiceClient Client { get; set; }
    }
}