using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;

[TestFixture]
class ValidUntilV1BlobStorageDataBusTests : ValidUntilTest
{
    //https://github.com/Particular/NServiceBus.Azure/blob/ba6b0de53255072764f9aaf433c6487e12bc41ed/src/impl/databus/NServiceBus.DataBus.Azure.BlobStorage/BlobStorageDataBus.cs#L46
    protected override void SetValidUntil(ICloudBlob cloudBlob, TimeSpan timeToBeReceived)
    {
        cloudBlob.Metadata["ValidUntil"] = (DateTime.Now + timeToBeReceived).ToString();
    }

    [Ignore("this never worked since TimeSpan.MaxValue would overflow the datetime math")]
    public override async Task ValidUntil_defaults_to_DateTimeMax()
    {
        await base.ValidUntil_defaults_to_DateTimeMax();
    }

    [Ignore("no way this can work since we cannot be sure what culture the value was writen in")]
    public override async Task ValidUntil_is_not_corrupt_by_change_in_local()
    {
        await base.ValidUntil_is_not_corrupt_by_change_in_local();
    }

    [Ignore("this never worked since TimeSpan.MaxValue would overflow the datetime math")]
    public override async Task ValidUntil_defaults_to_DefaultTtl_IfDefaultTtlSet()
    {
        await base.ValidUntil_defaults_to_DefaultTtl_IfDefaultTtlSet();
    }

    [Ignore("this never worked since TimeSpan.MaxValue would overflow the datetime math")]
    public override async Task ValidUntil_defaults_to_DateTimeMax_IfDefaultTtlSet_ButNoLastModifiedDateSet()
    {
        await base.ValidUntil_defaults_to_DateTimeMax_IfDefaultTtlSet_ButNoLastModifiedDateSet();
    }
}