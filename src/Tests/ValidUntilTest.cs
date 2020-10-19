using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NServiceBus.DataBus.AzureBlobStorage;
using NUnit.Framework;

abstract class ValidUntilTest
{
    [Test]
    public async Task ValidUntil_is_correctly_set()
    {
        var cloudBlob = StubACloudBlob();

        SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob);

        Assert.That(resultValidUntil, Is.EqualTo(DateTimeOffset.UtcNow.AddHours(1))
            .Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public virtual async Task ValidUntil_is_not_corrupt_by_change_in_local()
    {
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var cloudBlob = StubACloudBlob();
            var dateTime = new DateTime(2100, 1, 4, 12, 0, 0);
            var timeSpan = dateTime - DateTimeOffset.UtcNow;
            SetValidUntil(cloudBlob, timeSpan);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-AU");
            var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob);
            //Verify that day and month are not switched
            Assert.AreEqual(4, resultValidUntil.Day);
            Assert.AreEqual(1, resultValidUntil.Month);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }
    }

    [Test]
    public async Task ValidUntil_should_default_to_DateTimeMax_for_corrupted_value()
    {
        var cloudBlob = StubACloudBlob();
        // When testing "ValidUntil" metadata, can't set "ValidUntilUtc" as it trumps "ValidUntil" case.
        //SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        //HACK: set ValidUntil to be a non parsable string
        cloudBlob.Metadata["ValidUntil"] = "Not a date time";
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob);
        Assert.AreEqual(DateTimeOffset.MaxValue, resultValidUntil);
    }

    [Test]
    public virtual async Task ValidUntil_defaults_to_DateTimeMax()
    {
        var cloudBlob = StubACloudBlob();

        SetValidUntil(cloudBlob, TimeSpan.MaxValue);
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob);
        Assert.AreEqual(DateTimeOffset.MaxValue, resultValidUntil);
    }

    [Test]
    public virtual async Task ValidUntil_defaults_to_DefaultTtl_IfDefaultTtlSet()
    {
        var validUntil = DateTimeOffset.UtcNow;
        var cloudBlob = StubACloudBlob(validUntil);

        const int defaultTtl = 1;
        SetValidUntil(cloudBlob, TimeSpan.MaxValue);
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob, defaultTtl);
        Assert.AreEqual(validUntil.AddSeconds(defaultTtl), resultValidUntil);
    }

    [Test]
    public virtual async Task ValidUntil_defaults_to_DateTimeMax_IfDefaultTtlSet_ButNoLastModifiedDateSet()
    {
        var cloudBlob = StubACloudBlob();

        const int defaultTtl = 1;
        SetValidUntil(cloudBlob, TimeSpan.MaxValue);
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob, defaultTtl);
        Assert.AreEqual(DateTimeOffset.MaxValue, resultValidUntil);
    }

    [Test]
    public virtual async Task ValidUntil_is_respected_IfDefaultTtlSet()
    {
        var lastModified = DateTimeOffset.UtcNow;
        var cloudBlob = StubACloudBlob(lastModified);

        const int defaultTtl = 1;
        SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob, defaultTtl);

        Assert.That(resultValidUntil, Is.EqualTo(DateTimeOffset.UtcNow.AddHours(1))
            .Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public virtual async Task ValidUntil_is_respected_IfDefaultTtlSet_EvenWhenNoLastModifiedDateFound()
    {
        var cloudBlob = StubACloudBlob();

        const int defaultTtl = 1;
        SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob, defaultTtl);

        Assert.That(resultValidUntil, Is.EqualTo(DateTimeOffset.UtcNow.AddHours(1))
            .Within(TimeSpan.FromSeconds(10)));
    }

    protected ICloudBlob StubACloudBlob(DateTimeOffset? lastModified = default(DateTimeOffset?))
    {
        var cloudBlobProperties = new BlobProperties();
        var property = typeof(BlobProperties).GetProperty("LastModified");
        property.SetValue(cloudBlobProperties, lastModified, BindingFlags.NonPublic, null, null, null);


        var cloudBlob = new FakeCloudBlob();
        cloudBlob.Properties = cloudBlobProperties;
        return cloudBlob;
    }

    protected abstract void SetValidUntil(ICloudBlob cloudBlob, TimeSpan timeSpan);
    
    class FakeCloudBlob : BlobClient
    {
        public override Task<Response<BlobInfo>> SetMetadataAsync(IDictionary<string, string> metadata, BlobRequestConditions conditions = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<Response<BlobInfo>>(null);
        }
    }
}