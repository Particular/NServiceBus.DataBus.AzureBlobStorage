﻿namespace NServiceBus.ClaimCheck.AzureBlobStorage.Config
{
    using System;
    using Azure.Storage.Blobs;

    class ThrowIfNoBlobServiceClientProvider : NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient
    {
        public BlobServiceClient Client => throw new Exception($"No BlobServiceClient has been configured. Either provide a connection string, use `persistence.UseBlobServiceClient(client)` or register an implementation of `{nameof(IProvideBlobServiceClient)}` in the container.");
    }
}