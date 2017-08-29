namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    class BlobStorageDataBus : IDataBus, IDisposable
    {
        public BlobStorageDataBus(CloudBlobContainer container, DataBusSettings settings, IAsyncTimer timer)
        {
            this.container = container;
            this.settings = settings;
            this.timer = timer;

            retryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(settings.BackOffInterval), settings.MaxRetries);
        }

        public async Task<Stream> Get(string key)
        {
            var blob = container.GetBlockBlobReference(Path.Combine(settings.BasePath, key));
            await blob.FetchAttributesAsync().ConfigureAwait(false);

            var stream = new MemoryStream((int) blob.Properties.Length);

            blob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = settings.NumberOfIOThreads;
            container.ServiceClient.DefaultRequestOptions.RetryPolicy = retryPolicy;

            await blob.DownloadToStreamAsync(stream).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public async Task<string> Put(Stream stream, TimeSpan timeToBeReceived)
        {
            var key = Guid.NewGuid().ToString();
            var blob = container.GetBlockBlobReference(Path.Combine(settings.BasePath, key));
            SetValidUntil(blob, timeToBeReceived);
            blob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = settings.NumberOfIOThreads;
            container.ServiceClient.DefaultRequestOptions.RetryPolicy = retryPolicy;
            blob.StreamWriteSizeInBytes = settings.BlockSize;
            await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
            return key;
        }

        public async Task Start()
        {
            ServicePointManager.DefaultConnectionLimit = settings.NumberOfIOThreads;
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            if (settings.ShouldPerformCleanup())
            {
                timer.Start(DeleteExpiredBlobs, TimeSpan.FromMilliseconds(settings.CleanupInterval), exception =>
                {
                    logger.Error("Error deleting expired blobs", exception);
                });
            }

            logger.Info("Blob storage data bus started. Location: " + Path.Combine(container.Uri.ToString(), settings.BasePath));
        }

        public void Dispose()
        {
            timer.Stop().GetAwaiter().GetResult();
            logger.Info("Blob storage data bus stopped");
        }

        async Task DeleteExpiredBlobs()
        {            
            try
            {
                var blobs = await container.ListBlobsAsync().ConfigureAwait(false);

                foreach (var blockBlob in blobs.Select(blob => blob as CloudBlockBlob))
                {
                    if (blockBlob == null) continue;

                    await blockBlob.FetchAttributesAsync().ConfigureAwait(false);
                    var validUntil = await GetValidUntil(blockBlob, settings.TTL).ConfigureAwait(false);
                    if (validUntil < DateTime.UtcNow)
                    {
                        await blockBlob.DeleteIfExistsAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (StorageException ex) // needs to stay as it runs on a background thread
            {
                logger.Warn(ex.Message);
            }
        }


        internal static void SetValidUntil(ICloudBlob blob, TimeSpan timeToBeReceived)
        {
            if (timeToBeReceived != TimeSpan.MaxValue)
            {
                var validUntil = DateTime.UtcNow + timeToBeReceived;
                blob.Metadata["ValidUntilUtc"] = DateTimeExtensions.ToWireFormattedString(validUntil);
            }
            // else no ValidUntil will be considered it to be non-expiring or subject to maximum ttl
        }


        internal static async Task<DateTime> GetValidUntil(ICloudBlob blockBlob, long defaultTtl = long.MaxValue)
        {
            string validUntilUtcString;
            if (blockBlob.Metadata.TryGetValue("ValidUntilUtc", out validUntilUtcString))
            {
                return DateTimeExtensions.ToUtcDateTime(validUntilUtcString);
            }

            string validUntilString;
            if (!blockBlob.Metadata.TryGetValue("ValidUntil", out validUntilString))
            {
                // no ValidUntil and no ValidUntilUtc will be considered non-expiring or whatever default ttl is set
                return ToDefault(defaultTtl, blockBlob);
            }
            var style = DateTimeStyles.AssumeUniversal;
            if (!blockBlob.Metadata.ContainsKey("ValidUntilKind"))
            {
                style = DateTimeStyles.AdjustToUniversal;
            }

            DateTime validUntil;
            //since this is the old version that could be written in any culture we cannot be certain it will parse so need to handle failure
            if (!DateTime.TryParse(validUntilString, null, style, out validUntil))
            {
                var message = string.Format("Could not parse the 'ValidUntil' value `{0}` for blob {1}. Resetting 'ValidUntil' to not expire. You may consider manually removing this entry if non-expiry is incorrect.", validUntilString, blockBlob.Uri);
                logger.Error(message);
                //If we cant parse the datetime then assume data corruption and store for max time
                SetValidUntil(blockBlob, TimeSpan.MaxValue);
                //upload the changed metadata
                await blockBlob.SetMetadataAsync().ConfigureAwait(false);

                return ToDefault(defaultTtl, blockBlob);
            }

            return validUntil.ToUniversalTime();
        }

        static DateTime ToDefault(long defaultTtl, ICloudBlob blockBlob)
        {
            if (defaultTtl != long.MaxValue && blockBlob.Properties.LastModified.HasValue)
            {
                try
                {
                    return blockBlob.Properties.LastModified.Value.Add(TimeSpan.FromSeconds(defaultTtl)).UtcDateTime;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // fallback to datetime.maxvalue
                }
            }

            return DateTime.MaxValue;
        }

        CloudBlobContainer container;
        DataBusSettings settings;
        IAsyncTimer timer;
        ExponentialRetry retryPolicy;
        static ILog logger = LogManager.GetLogger(typeof(IDataBus));
    }
}