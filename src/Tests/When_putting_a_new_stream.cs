using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NServiceBus.DataBus.AzureBlobStorage.Config;
using NUnit.Framework;

namespace NServiceBus.DataBus.AzureBlobStorage.Tests
{
    [TestFixture]
    public class When_putting_a_new_stream
    {
        [Test]
        public async Task Should_work()
        {
            var blobServiceClient = new BlobServiceClient(GetEnvConfiguredConnectionString());
            var blobStorageDataBus = new BlobStorageDataBus(new BlobServiceClientProvidedByConfiguration
            {
                Client = blobServiceClient
            }, new DataBusSettings
            {
                Container = "test"
            }, new AsyncTimer());
            
            var payloadToSend = new byte[1024 * 1024];
            new Random().NextBytes(payloadToSend);
            var databusProperty = new DataBusProperty<byte[]>(payloadToSend);
            var stream = new MemoryStream(databusProperty.Value.Length);

            var blobKey = await blobStorageDataBus.Put(stream, TimeSpan.FromSeconds(2));

            var blob = await blobStorageDataBus.Get(blobKey);
            Assert.That(blob, Is.Not.Null);

        }
        
        public static string GetEnvConfiguredConnectionString()
        {
            var environmentVartiableName = "NServiceBus_DataBus_AzureBlobStorage_ConnectionString";
            var connectionString = GetEnvironmentVariable(environmentVartiableName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception($"Oh no! We couldn't find an environment variable '{environmentVartiableName}' with Azure Storage connection string.");
            }

            return connectionString;
        }

        static string GetEnvironmentVariable(string variable)
        {
            var candidate = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
            return string.IsNullOrWhiteSpace(candidate) ? Environment.GetEnvironmentVariable(variable) : candidate;
        }
    }
}