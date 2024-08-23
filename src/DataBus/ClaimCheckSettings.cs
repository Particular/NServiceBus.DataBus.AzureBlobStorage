namespace NServiceBus.ClaimCheck.AzureBlobStorage
{
    class ClaimCheckSettings
    {
        public ClaimCheckSettings()
        {
            Container = "databus";
            BasePath = "";
            MaxRetries = 5;
            NumberOfIOThreads = 1;
            BackOffInterval = 30; // seconds
            ConnectionString = "UseDevelopmentStorage=true";
            ConnectionStringProvided = false;
        }

        public string Container;
        public int MaxRetries;
        public int BackOffInterval;
        public int NumberOfIOThreads;
        public string BasePath;
        public string ConnectionString;
        public bool ConnectionStringProvided;
    }
}