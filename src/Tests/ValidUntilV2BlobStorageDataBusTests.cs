using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NUnit.Framework;

[TestFixture]
class ValidUntilV2BlobStorageDataBusTests : ValidUntilTest
{
    //https://github.com/Particular/NServiceBus.Azure/blob/e9db29beb21d1fd914191e479cb5948fffd92f3b/src/NServiceBus.Azure/DataBus/Azure/BlobStorage/BlobStorageDataBus.cs#L41
    protected override void SetValidUntil(BlobClient blobClient, TimeSpan timeToBeReceived)
    {
        var properties = blobClient.GetProperties();
        if (timeToBeReceived == TimeSpan.MaxValue)
        {
            properties.Value.Metadata["ValidUntil"] = TimeSpan.MaxValue.ToString();
        }
        else
        {
            properties.Value.Metadata["ValidUntil"] = (DateTime.Now + timeToBeReceived).ToString();
        }
    }

    [Ignore("no way this can work since we cannot be sure what culture the value was writen in")]
    public override async Task ValidUntil_is_not_corrupt_by_change_in_local()
    {
        await base.ValidUntil_is_not_corrupt_by_change_in_local();
    }
}