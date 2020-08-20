using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
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

        Assert.That(resultValidUntil, Is.EqualTo(DateTime.UtcNow.AddHours(1))
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
            var timeSpan = dateTime - DateTime.UtcNow;
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
        Assert.AreEqual(DateTime.MaxValue, resultValidUntil);
    }

    [Test]
    public async Task ValidUntil_is_UtcKind()
    {
        var cloudBlob = StubACloudBlob();
        SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob);
        Assert.AreEqual(DateTimeKind.Utc, resultValidUntil.Kind);
    }

    [Test]
    public virtual async Task ValidUntil_defaults_to_DateTimeMax()
    {
        var cloudBlob = StubACloudBlob();

        SetValidUntil(cloudBlob, TimeSpan.MaxValue);
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob);
        Assert.AreEqual(DateTime.MaxValue, resultValidUntil);
    }

    [Test]
    public virtual async Task ValidUntil_defaults_to_DefaultTtl_IfDefaultTtlSet()
    {
        var validUntil = DateTime.UtcNow;
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
        Assert.AreEqual(DateTime.MaxValue, resultValidUntil);
    }

    [Test]
    public virtual async Task ValidUntil_is_respected_IfDefaultTtlSet()
    {
        var lastModified = DateTime.UtcNow;
        var cloudBlob = StubACloudBlob(lastModified);

        const int defaultTtl = 1;
        SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob, defaultTtl);

        Assert.That(resultValidUntil, Is.EqualTo(DateTime.UtcNow.AddHours(1))
            .Within(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public virtual async Task ValidUntil_is_respected_IfDefaultTtlSet_EvenWhenNoLastModifiedDateFound()
    {
        var cloudBlob = StubACloudBlob();

        const int defaultTtl = 1;
        SetValidUntil(cloudBlob, TimeSpan.FromHours(1));
        var resultValidUntil = await BlobStorageDataBus.GetValidUntil(cloudBlob, defaultTtl);

        Assert.That(resultValidUntil, Is.EqualTo(DateTime.UtcNow.AddHours(1))
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

    class FakeCloudBlob : ICloudBlob
    {
#if NET472
        public Uri Uri { get; }
        public StorageUri StorageUri { get; }
        public CloudBlobDirectory Parent { get; }
        public CloudBlobContainer Container { get; }
        public Stream OpenRead(AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginOpenRead(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginOpenRead(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public Stream EndOpenRead(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void UploadFromStream(Stream source, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public void UploadFromStream(Stream source, long length, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromStream(Stream source, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromStream(Stream source, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromStream(Stream source, long length, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromStream(Stream source, long length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndUploadFromStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, long length)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, long length, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, long length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, long length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void UploadFromFile(string path, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromFile(string path, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromFile(string path, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndUploadFromFile(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromFileAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromFileAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromFileAsync(string path, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromFileAsync(string path, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void UploadFromByteArray(byte[] buffer, int index, int count, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromByteArray(byte[] buffer, int index, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginUploadFromByteArray(byte[] buffer, int index, int count, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndUploadFromByteArray(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void DownloadToStream(Stream target, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadToStream(Stream target, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadToStream(Stream target, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndDownloadToStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToStreamAsync(Stream target)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToStreamAsync(Stream target, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToStreamAsync(Stream target, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToStreamAsync(Stream target, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void DownloadToFile(string path, FileMode mode, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadToFile(string path, FileMode mode, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadToFile(string path, FileMode mode, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndDownloadToFile(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToFileAsync(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToFileAsync(string path, FileMode mode, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToFileAsync(string path, FileMode mode, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToFileAsync(string path, FileMode mode, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public int DownloadToByteArray(byte[] target, int index, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadToByteArray(byte[] target, int index, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadToByteArray(byte[] target, int index, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public int EndDownloadToByteArray(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void DownloadRangeToStream(Stream target, long? offset, long? length, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadRangeToStream(Stream target, long? offset, long? length, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadRangeToStream(Stream target, long? offset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndDownloadRangeToStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task DownloadRangeToStreamAsync(Stream target, long? offset, long? length)
        {
            throw new NotImplementedException();
        }

        public Task DownloadRangeToStreamAsync(Stream target, long? offset, long? length, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DownloadRangeToStreamAsync(Stream target, long? offset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DownloadRangeToStreamAsync(Stream target, long? offset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public int DownloadRangeToByteArray(byte[] target, int index, long? blobOffset, long? length, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadRangeToByteArray(byte[] target, int index, long? blobOffset, long? length, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDownloadRangeToByteArray(byte[] target, int index, long? blobOffset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public int EndDownloadRangeToByteArray(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadRangeToByteArrayAsync(byte[] target, int index, long? blobOffset, long? length)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadRangeToByteArrayAsync(byte[] target, int index, long? blobOffset, long? length, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadRangeToByteArrayAsync(byte[] target, int index, long? blobOffset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadRangeToByteArrayAsync(byte[] target, int index, long? blobOffset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool Exists(BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginExists(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginExists(BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public bool EndExists(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void FetchAttributes(AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginFetchAttributes(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginFetchAttributes(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndFetchAttributes(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task FetchAttributesAsync()
        {
            throw new NotImplementedException();
        }

        public Task FetchAttributesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task FetchAttributesAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task FetchAttributesAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SetMetadata(AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginSetMetadata(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginSetMetadata(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndSetMetadata(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task SetMetadataAsync()
        {
            return Task.FromResult(0);
        }

        public Task SetMetadataAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetMetadataAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task SetMetadataAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void SetProperties(AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginSetProperties(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginSetProperties(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndSetProperties(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Delete(DeleteSnapshotsOption deleteSnapshotsOption = DeleteSnapshotsOption.None, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDelete(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDelete(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndDelete(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool DeleteIfExists(DeleteSnapshotsOption deleteSnapshotsOption = DeleteSnapshotsOption.None, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDeleteIfExists(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginDeleteIfExists(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public bool EndDeleteIfExists(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIfExistsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIfExistsAsync(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIfExistsAsync(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string AcquireLease(TimeSpan? leaseTime, string proposedLeaseId, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginAcquireLease(TimeSpan? leaseTime, string proposedLeaseId, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginAcquireLease(TimeSpan? leaseTime, string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public string EndAcquireLease(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<string> AcquireLeaseAsync(TimeSpan? leaseTime, string proposedLeaseId = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> AcquireLeaseAsync(TimeSpan? leaseTime, string proposedLeaseId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> AcquireLeaseAsync(TimeSpan? leaseTime, string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<string> AcquireLeaseAsync(TimeSpan? leaseTime, string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void RenewLease(AccessCondition accessCondition, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginRenewLease(AccessCondition accessCondition, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginRenewLease(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndRenewLease(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task RenewLeaseAsync(AccessCondition accessCondition)
        {
            throw new NotImplementedException();
        }

        public Task RenewLeaseAsync(AccessCondition accessCondition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RenewLeaseAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task RenewLeaseAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string ChangeLease(string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginChangeLease(string proposedLeaseId, AccessCondition accessCondition, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginChangeLease(string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public string EndChangeLease(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<string> ChangeLeaseAsync(string proposedLeaseId, AccessCondition accessCondition)
        {
            throw new NotImplementedException();
        }

        public Task<string> ChangeLeaseAsync(string proposedLeaseId, AccessCondition accessCondition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> ChangeLeaseAsync(string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<string> ChangeLeaseAsync(string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void ReleaseLease(AccessCondition accessCondition, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginReleaseLease(AccessCondition accessCondition, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginReleaseLease(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndReleaseLease(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseLeaseAsync(AccessCondition accessCondition)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseLeaseAsync(AccessCondition accessCondition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseLeaseAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseLeaseAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public TimeSpan BreakLease(TimeSpan? breakPeriod = null, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginBreakLease(TimeSpan? breakPeriod, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginBreakLease(TimeSpan? breakPeriod, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public TimeSpan EndBreakLease(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> BreakLeaseAsync(TimeSpan? breakPeriod)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> BreakLeaseAsync(TimeSpan? breakPeriod, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> BreakLeaseAsync(TimeSpan? breakPeriod, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> BreakLeaseAsync(TimeSpan? breakPeriod, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void AbortCopy(string copyId, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginAbortCopy(string copyId, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginAbortCopy(string copyId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndAbortCopy(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task AbortCopyAsync(string copyId)
        {
            throw new NotImplementedException();
        }

        public Task AbortCopyAsync(string copyId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AbortCopyAsync(string copyId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task AbortCopyAsync(string copyId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginGetAccountProperties(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public ICancellableAsyncResult BeginGetAccountProperties(BlobRequestOptions requestOptions, OperationContext operationContext, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public AccountProperties EndGetAccountProperties(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync(BlobRequestOptions requestOptions, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync(BlobRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public AccountProperties GetAccountProperties(BlobRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, string groupPolicyIdentifier)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, string groupPolicyIdentifier)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, string groupPolicyIdentifier, SharedAccessProtocol? protocols, IPAddressOrRange ipAddressOrRange)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }
        public CloudBlobClient ServiceClient { get; }
        public int StreamWriteSizeInBytes { get; set; }
        public int StreamMinimumReadSizeInBytes { get; set; }
        public BlobProperties Properties { get; set; }
        public IDictionary<string, string> Metadata {get; } = new Dictionary<string, string>();
        public DateTimeOffset? SnapshotTime { get; }
        public bool IsSnapshot { get; }
        public Uri SnapshotQualifiedUri { get; }
        public StorageUri SnapshotQualifiedStorageUri { get; }
        public CopyState CopyState { get; }
        public BlobType BlobType { get; }
#elif NETCOREAPP
        public Uri Uri { get; }

        public StorageUri StorageUri { get; }
        public CloudBlobDirectory Parent { get; }
        public CloudBlobContainer Container { get; }
        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, string groupPolicyIdentifier)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, string groupPolicyIdentifier)
        {
            throw new NotImplementedException();
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers, string groupPolicyIdentifier, SharedAccessProtocol? protocols, IPAddressOrRange ipAddressOrRange)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, long length)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromStreamAsync(Stream source, long length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromFileAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromFileAsync(string path, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public Task UploadFromByteArrayAsync(byte[] buffer, int index, int count, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToStreamAsync(Stream target)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToStreamAsync(Stream target, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToFileAsync(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public Task DownloadToFileAsync(string path, FileMode mode, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadToByteArrayAsync(byte[] target, int index, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DownloadRangeToStreamAsync(Stream target, long? offset, long? length)
        {
            throw new NotImplementedException();
        }

        public Task DownloadRangeToStreamAsync(Stream target, long? offset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadRangeToByteArrayAsync(byte[] target, int index, long? blobOffset, long? length)
        {
            throw new NotImplementedException();
        }

        public Task<int> DownloadRangeToByteArrayAsync(byte[] target, int index, long? blobOffset, long? length, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task FetchAttributesAsync()
        {
            throw new NotImplementedException();
        }

        public Task FetchAttributesAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task SetMetadataAsync()
        {
            return Task.FromResult(0);
        }

        public Task SetMetadataAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetPropertiesAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIfExistsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIfExistsAsync(DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<string> AcquireLeaseAsync(TimeSpan? leaseTime, string proposedLeaseId = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> AcquireLeaseAsync(TimeSpan? leaseTime, string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task RenewLeaseAsync(AccessCondition accessCondition)
        {
            throw new NotImplementedException();
        }

        public Task RenewLeaseAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<string> ChangeLeaseAsync(string proposedLeaseId, AccessCondition accessCondition)
        {
            throw new NotImplementedException();
        }

        public Task<string> ChangeLeaseAsync(string proposedLeaseId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseLeaseAsync(AccessCondition accessCondition)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseLeaseAsync(AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> BreakLeaseAsync(TimeSpan? breakPeriod)
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> BreakLeaseAsync(TimeSpan? breakPeriod, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task AbortCopyAsync(string copyId)
        {
            throw new NotImplementedException();
        }

        public Task AbortCopyAsync(string copyId, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync(BlobRequestOptions requestOptions, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public Task<AccountProperties> GetAccountPropertiesAsync(BlobRequestOptions requestOptions, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }
        public CloudBlobClient ServiceClient { get; }
        public int StreamWriteSizeInBytes { get; set; }
        public int StreamMinimumReadSizeInBytes { get; set; }
        public BlobProperties Properties { get; set; }
        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
        public DateTimeOffset? SnapshotTime { get; }
        public bool IsSnapshot { get; }
        public Uri SnapshotQualifiedUri { get; }
        public StorageUri SnapshotQualifiedStorageUri { get; }
        public CopyState CopyState { get; }
        public BlobType BlobType { get; }
#endif
    }
}