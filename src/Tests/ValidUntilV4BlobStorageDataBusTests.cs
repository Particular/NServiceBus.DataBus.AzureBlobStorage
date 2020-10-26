using System;
using Azure.Storage.Blobs;
using NServiceBus.DataBus.AzureBlobStorage;
using NUnit.Framework;

[TestFixture]
class ValidUntilV4BlobStorageDataBusTests : ValidUntilTest
{
    protected override void SetValidUntil(BlobClient blobClient, TimeSpan timeToBeReceived)
    {
        BlobStorageDataBus.SetValidUntil(blobClient, timeToBeReceived);
    }
}