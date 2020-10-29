namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Logging;
    using Azure;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Config;

    class BlobStorageDataBus : IDataBus, IDisposable
    {
        public BlobStorageDataBus(IProvideBlobServiceClient blobServiceClientProvider, DataBusSettings settings, IAsyncTimer timer)
        {
            this.settings = settings;
            this.timer = timer;
            
            blobContainerClient = blobServiceClientProvider.Client.GetBlobContainerClient(settings.Container);
        }

        public async Task<Stream> Get(string key)
        {
            var blobClient = blobContainerClient.GetBlobClient(Path.Combine(settings.BasePath, key));
            var properties = await blobClient.GetPropertiesAsync().ConfigureAwait(false);
            var stream = new MemoryStream((int) properties.Value.ContentLength);

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

            SetValidUntil(blobClient, timeToBeReceived);

            //blobClient.StreamWriteSizeInBytes = settings.BlockSize;
            var blobUploadOptions = new BlobUploadOptions
            {
                TransferOptions = new StorageTransferOptions
                {
                    MaximumConcurrency = settings.NumberOfIOThreads
                }
            };
            await blobClient.UploadAsync(stream, blobUploadOptions).ConfigureAwait(false);

            return key;
        }

        public async Task Start()
        {
            await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            if (settings.ShouldPerformCleanup())
            {
                timer.Start(DeleteExpiredBlobs, TimeSpan.FromMilliseconds(settings.CleanupInterval), exception =>
                {
                    logger.Error("Error deleting expired blobs", exception);
                });
            }

            logger.Info("Blob storage data bus started. Location: " + Path.Combine(blobContainerClient.Uri.ToString(), settings.BasePath));
        }

        public void Dispose()
        {
            timer.Stop().GetAwaiter().GetResult();
            logger.Info("Blob storage data bus stopped");
        }

        async Task DeleteExpiredBlobs()
        {
            string continuationToken = null;

            try
            {
                // Call the listing operation and enumerate the result segment.
                // When the continuation token is empty, the last segment has been returned
                // and execution can exit the loop.
                do
                {
                    var resultSegment = blobContainerClient.GetBlobs().AsPages(continuationToken);
                    foreach (Page<BlobItem> blobPage in resultSegment)
                    {
                        foreach (BlobItem blobItem in blobPage.Values)
                        {
                            var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                            var validUntil = await GetValidUntil(blobClient, settings.TTL).ConfigureAwait(false);
                            if (validUntil < DateTimeOffset.UtcNow)
                            {
                                await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
                            }
                        }

                        // Get the continuation token and loop until it is empty.
                        continuationToken = blobPage.ContinuationToken;
                    }

                }
                while (continuationToken != string.Empty);

            }
            catch (RequestFailedException ex)
            {
                logger.WarnFormat($"{nameof(BlobStorageDataBus)} has encountered an exception.", ex);
            }
        }

        internal static async void SetValidUntil(BlobClient blobClient, TimeSpan timeToBeReceived)
        {
            if (timeToBeReceived != TimeSpan.MaxValue)
            {
                var validUntil = DateTimeOffset.UtcNow + timeToBeReceived;
                var properties = await blobClient.GetPropertiesAsync().ConfigureAwait(false);
                properties.Value.Metadata["ValidUntilUtc"] = DateTimeOffsetHelper.ToWireFormattedString(validUntil);
                await blobClient.SetMetadataAsync(properties.Value.Metadata).ConfigureAwait(false);
            }
            // else no ValidUntil will be considered it to be non-expiring or subject to maximum ttl
        }

        internal static async Task<DateTimeOffset> GetValidUntil(BlobClient blobClient, long defaultTtl = long.MaxValue)
        {
            var properties = await blobClient.GetPropertiesAsync().ConfigureAwait(false);
            var metadata = properties.Value.Metadata;
            if (metadata.TryGetValue("ValidUntilUtc", out var validUntilUtcString))
            {
                return DateTimeOffsetHelper.ToDateTimeOffset(validUntilUtcString);
            }

            if (!metadata.TryGetValue("ValidUntil", out var validUntilString))
            {
                // no ValidUntil and no ValidUntilUtc will be considered non-expiring or whatever default ttl is set
                var defaultedTtl = await ToDefault(defaultTtl, blobClient).ConfigureAwait(false);
                return defaultedTtl;
            }
            var style = DateTimeStyles.AssumeUniversal;
            if (!metadata.ContainsKey("ValidUntilKind"))
            {
                style = DateTimeStyles.AdjustToUniversal;
            }

            //since this is the old version that could be written in any culture we cannot be certain it will parse so need to handle failure
            if (!DateTimeOffset.TryParse(validUntilString, null, style, out var validUntil))
            {
                 var message = $"Could not parse the 'ValidUntil' value `{validUntilString}` for blob {blobClient.Uri}. Resetting 'ValidUntil' to not expire. You may consider manually removing this entry if non-expiry is incorrect.";
                 logger.Error(message);
                 // If we cant parse the datetime then assume data corruption and store for max time
                 SetValidUntil(blobClient, TimeSpan.MaxValue);
                 // upload the changed metadata
                 await blobClient.SetMetadataAsync(metadata).ConfigureAwait(false);

                 var defaultedTtl = await ToDefault(defaultTtl, blobClient).ConfigureAwait(false);
                 return defaultedTtl;
            }

            return validUntil.ToUniversalTime();
        }

        static async Task<DateTimeOffset> ToDefault(long defaultTtl, BlobClient blobClient)
        {
            var properties = await blobClient.GetPropertiesAsync().ConfigureAwait(false);
            if (defaultTtl != long.MaxValue && properties.Value.LastModified != DateTimeOffset.MinValue)
            {
                try
                {
                    return properties.Value.LastModified.Add(TimeSpan.FromSeconds(defaultTtl)).UtcDateTime;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // fallback to datetime.maxvalue
                }
            }

            return DateTimeOffset.MaxValue;
        }

        BlobContainerClient blobContainerClient;
        DataBusSettings settings;
        IAsyncTimer timer;
        static ILog logger = LogManager.GetLogger(typeof(IDataBus));
    }
}