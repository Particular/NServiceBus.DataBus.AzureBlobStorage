namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System.Collections.Generic;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Logging;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Microsoft.IO;
    using Config;

    class BlobStorageDataBus : IDataBus, IDisposable
    {
        public BlobStorageDataBus(IProvideBlobServiceClient blobServiceClientProvider, DataBusSettings settings)
        {
            this.settings = settings;

            blobContainerClient = blobServiceClientProvider.Client.GetBlobContainerClient(settings.Container);
        }

        public async Task<Stream> Get(string key)
        {
            var blobClient = blobContainerClient.GetBlobClient(Path.Combine(settings.BasePath, key));
            var properties = await blobClient.GetPropertiesAsync().ConfigureAwait(false);
            // core takes care of disposing
            var stream = memoryStreamManager.GetStream(key, (int) properties.Value.ContentLength);

            var transferOptions = new StorageTransferOptions
            {
                MaximumConcurrency = settings.NumberOfIOThreads,
            };

            await blobClient.DownloadToAsync(stream, null, transferOptions).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public async Task<string> Put(Stream stream, TimeSpan timeToBeReceived)
        {
            var key = Guid.NewGuid().ToString();
            var blobClient = blobContainerClient.GetBlobClient(Path.Combine(settings.BasePath, key));

            Dictionary<string, string> metadata = null;
            if (timeToBeReceived != TimeSpan.MaxValue)
            {
                var validUntil = DateTimeOffset.UtcNow + timeToBeReceived;

                metadata = new Dictionary<string, string>
                {
                    { "ValidUntilUtc", DateTimeOffsetHelper.ToWireFormattedString(validUntil) }
                };
            }
            var blobUploadOptions = new BlobUploadOptions
            {
                TransferOptions = new StorageTransferOptions
                {
                    MaximumConcurrency = settings.NumberOfIOThreads
                },
                Metadata = metadata
            };
            await blobClient.UploadAsync(stream, blobUploadOptions).ConfigureAwait(false);

            return key;
        }

        public async Task Start()
        {
            await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            logger.Info("Blob storage data bus started. Location: " + Path.Combine(blobContainerClient.Uri.ToString(), settings.BasePath));
        }

        public void Dispose()
        {
            logger.Info("Blob storage data bus stopped");
        }

        BlobContainerClient blobContainerClient;
        DataBusSettings settings;
        static ILog logger = LogManager.GetLogger(typeof(IDataBus));
        static readonly RecyclableMemoryStreamManager memoryStreamManager = new RecyclableMemoryStreamManager();
    }
}