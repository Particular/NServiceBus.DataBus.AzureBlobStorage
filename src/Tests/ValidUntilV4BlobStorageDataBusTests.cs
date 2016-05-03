using System;
using Microsoft.WindowsAzure.Storage.Blob;
using NServiceBus.DataBus.AzureBlobStorage;
using NUnit.Framework;

[TestFixture]
class ValidUntilV4BlobStorageDataBusTests : ValidUntilTest
{
    protected override void SetValidUntil(ICloudBlob cloudBlob, TimeSpan timeToBeReceived)
    {
        BlobStorageDataBus.SetValidUntil(cloudBlob, timeToBeReceived);
    }
}