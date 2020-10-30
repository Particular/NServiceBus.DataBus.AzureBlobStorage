using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NServiceBus;
using NServiceBus.DataBus.AzureBlobStorage;
using NServiceBus.DataBus.AzureBlobStorage.Config;
using NUnit.Framework;

[TestFixture]
public class BlobStorageDataBusTests
{
    [Test]
    public async Task Put_MaxTimeToBeReceive_should_not_write_metadata()
    {
        var fakeBlobClient = new FakeBlobClient();

        var databus = new BlobStorageDataBus(new BlobServiceClientProvidedByConfiguration
        {
            Client = new FakeBobServiceClient(new FakeContainerClient(fakeBlobClient))
        }, new DataBusSettings(), new AsyncTimer());

        await databus.Put(Stream.Null, TimeSpan.MaxValue);

        Assert.IsNull(fakeBlobClient.UploadOptions.Metadata);
    }

    [Test]
    public async Task Put_TimeToBeReceived_should_write_metadata()
    {
        var fakeBlobClient = new FakeBlobClient();

        var databus = new BlobStorageDataBus(new BlobServiceClientProvidedByConfiguration
        {
            Client = new FakeBobServiceClient(new FakeContainerClient(fakeBlobClient))
        }, new DataBusSettings(), new AsyncTimer());

        await databus.Put(Stream.Null, TimeSpan.FromSeconds(30));

        var metadata = fakeBlobClient.UploadOptions.Metadata;

        Assert.That(metadata, Is.Not.Empty.And.ContainKey("ValidUntilUtc"));
    }

    class FakeBobServiceClient : BlobServiceClient
    {
        private FakeContainerClient containerClient;

        public FakeBobServiceClient(FakeContainerClient containerClient)
        {
            this.containerClient = containerClient;
        }

        public override BlobContainerClient GetBlobContainerClient(string blobContainerName)
        {
            return containerClient;
        }
    }

    class FakeContainerClient : BlobContainerClient
    {
        private FakeBlobClient blobClient;

        public FakeContainerClient(FakeBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public override BlobClient GetBlobClient(string blobName)
        {
            return blobClient;
        }
    }

    class FakeBlobClient : BlobClient
    {
        public BlobUploadOptions UploadOptions { get; set; }

        public override Task<Response<BlobContentInfo>> UploadAsync(Stream content, BlobUploadOptions options,
            CancellationToken cancellationToken = new CancellationToken())
        {
            UploadOptions = options;
            return Task.FromResult(default(Response<BlobContentInfo>));
        }
    }
}