namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    static class CloudBlobContainerEx
    {
        public static async Task<List<IListBlobItem>> ListBlobsAsync(this CloudBlobContainer container)
        {
            BlobContinuationToken token = null;
            var blobs = new List<IListBlobItem>();

            do
            {
                var segment = await container.ListBlobsSegmentedAsync(token).ConfigureAwait(false);
                token = segment.ContinuationToken;
                blobs.AddRange(segment.Results);
            } while (token != null);

            return blobs;
        }
    }
}
