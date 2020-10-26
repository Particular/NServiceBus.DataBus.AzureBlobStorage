using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NUnit.Framework;

[TestFixture]
class ValidUntilV3BlobStorageDataBusTests : ValidUntilTest
{
    //https://github.com/Particular/NServiceBus.Azure/blob/21ab8ab2e6fc413c4113b098c4156c64b48f860e/src/NServiceBus.Azure/DataBus/Azure/BlobStorage/BlobStorageDataBus.cs#L42
    protected override void SetValidUntil(BlobClient blobClient, TimeSpan timeToBeReceived)
    {
        var properties = blobClient.GetProperties();
        if (timeToBeReceived == TimeSpan.MaxValue)
        {
            properties.Value.Metadata["ValidUntil"] = TimeSpan.MaxValue.ToString();
        }
        else
        {
            properties.Value.Metadata["ValidUntil"] = (DateTime.UtcNow + timeToBeReceived).ToString();
        }
        properties.Value.Metadata["ValidUntilKind"] = "Utc";
    }

    [Ignore("no way this can work since we cannot be sure what culture the value was writen in")]
    public override async Task ValidUntil_is_not_corrupt_by_change_in_local()
    {
        await base.ValidUntil_is_not_corrupt_by_change_in_local();
    }
}