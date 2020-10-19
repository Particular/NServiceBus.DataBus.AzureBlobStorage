// using Azure.Storage.Blobs;
//
// namespace NServiceBus
// {
//     using System.Collections.Generic;
//     using System.Threading.Tasks;
//
//     static class CloudBlobContainerEx
//     {
//         public static async Task<List<IListBlobItem>> ListBlobsAsync(this BlobContainerClient container)
//         {
//             BlobContinuationToken token = null;
//             var blobs = new List<IListBlobItem>();
//
//             do
//             {
//                 var segment = await container.(token).ConfigureAwait(false);
//                 token = segment.ContinuationToken;
//                 blobs.AddRange(segment.Results);
//             }
//             while (token != null);
//
//             return blobs;
//         }
//     }
// }
