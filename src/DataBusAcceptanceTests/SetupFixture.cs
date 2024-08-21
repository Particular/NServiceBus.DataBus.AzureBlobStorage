namespace NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests
{
    using Azure.Storage.Blobs;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var connectionString = GetEnvConfiguredConnectionString();

            ContainerName = $"{Path.GetFileNameWithoutExtension(Path.GetTempFileName())}{DateTime.UtcNow.Ticks}".ToLowerInvariant();

            BlobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient = BlobServiceClient.GetBlobContainerClient(ContainerName);
            await BlobContainerClient.CreateIfNotExistsAsync();
        }

        [OneTimeTearDown]
        public Task OneTimeTearDown()
        {
            return BlobContainerClient.DeleteIfExistsAsync();
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

        public static string ContainerName;
        public static BlobContainerClient BlobContainerClient;
        public static BlobServiceClient BlobServiceClient;
    }
}