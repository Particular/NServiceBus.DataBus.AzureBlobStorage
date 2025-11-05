namespace NServiceBus.ClaimCheck.AzureBlobStorage
{
    class ClaimCheckSettings
    {
        public string Container = "databus";
        public int MaxRetries = 5;
        public int BackOffInterval = 30; // seconds
        public int NumberOfIOThreads = 1;
        public string BasePath = "";
        public string ConnectionString = "UseDevelopmentStorage=true";
        public bool ConnectionStringProvided = false;

        public IProvideBlobServiceClient CustomBlobServiceProvider;
    }
}