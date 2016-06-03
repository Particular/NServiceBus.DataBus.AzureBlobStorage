namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    class BlobStorageDataBus : IDataBus, IDisposable
    {
        public BlobStorageDataBus(CloudBlobContainer container, DataBusSettings settings)
        {
            this.container = container;
            this.settings = settings;
            timer = new Timer(o => DeleteExpiredBlobs());
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
            timer.Change(0, 300000);
            logger.Info("Blob storage data bus started. Location: " + Path.Combine(container.Uri.ToString(), settings.BasePath));
        }

        public void Dispose()
        {
            timer.Dispose();

            DeleteExpiredBlobs();

            logger.Info("Blob storage data bus stopped");
        }

        void DeleteExpiredBlobs()
        {
            try
            {
                var blobs = container.ListBlobs();
                foreach (var blockBlob in blobs.Select(blob => blob as CloudBlockBlob))
                {
                    if (blockBlob == null) continue;

                    blockBlob.FetchAttributes();
                    var validUntil = GetValidUntil(blockBlob, settings.TTL);
                    if (validUntil < DateTime.UtcNow)
                    {
                        blockBlob.DeleteIfExists();
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


        internal static DateTime GetValidUntil(ICloudBlob blockBlob, long defaultTtl = long.MaxValue)
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
                blockBlob.SetMetadata();

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
        Timer timer;
        ExponentialRetry retryPolicy;
        static ILog logger = LogManager.GetLogger(typeof(IDataBus));
    }
}