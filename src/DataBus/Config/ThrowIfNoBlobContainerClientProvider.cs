namespace NServiceBus.DataBus.AzureBlobStorage.Config
{
    using System;
    using Azure.Storage.Blobs;

    class ThrowIfNoBlobContainerClientProvider : IProvideBlobContainerClient
    {
        public BlobContainerClient Client => throw new Exception($"No blob container client has been configured. Either provide a connection string, use `persistence.UseBlobContainerClient(client)` or register an implementation of `{nameof(IProvideBlobContainerClient)}` in the container.");
    }
}