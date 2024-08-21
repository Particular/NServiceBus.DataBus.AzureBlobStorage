namespace NServiceBus.ClaimCheck.AzureBlobStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Logging;
    using Microsoft.IO;
    using NServiceBus.ClaimCheck;

    class BlobStorageClaimCheck : IClaimCheck, IDisposable
    {
        public BlobStorageClaimCheck(IProvideBlobServiceClient blobServiceClientProvider, ClaimCheckSettings settings)
        {
            this.settings = settings;

            blobContainerClient = blobServiceClientProvider.Client.GetBlobContainerClient(settings.Container);
        }

        public async Task<Stream> Get(string key, CancellationToken cancellationToken = default)
        {
            var blobClient = blobContainerClient.GetBlobClient(Path.Combine(settings.BasePath, key));
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            // core takes care of disposing
            var stream = memoryStreamManager.GetStream(key, (int)properties.Value.ContentLength);

            var transferOptions = new StorageTransferOptions
            {
                MaximumConcurrency = settings.NumberOfIOThreads,
            };

            await blobClient.DownloadToAsync(stream, null, transferOptions, cancellationToken).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public async Task<string> Put(Stream stream, TimeSpan timeToBeReceived, CancellationToken cancellationToken = default)
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
            await blobClient.UploadAsync(stream, blobUploadOptions, cancellationToken).ConfigureAwait(false);

            return key;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            logger.Info("Blob storage data bus started. Location: " + Path.Combine(blobContainerClient.Uri.ToString(), settings.BasePath));
        }

        public void Dispose()
        {
            logger.Info("Blob storage data bus stopped");
        }

        BlobContainerClient blobContainerClient;
        ClaimCheckSettings settings;
        static ILog logger = LogManager.GetLogger(typeof(IClaimCheck));
        static readonly RecyclableMemoryStreamManager memoryStreamManager = new RecyclableMemoryStreamManager();
    }
}