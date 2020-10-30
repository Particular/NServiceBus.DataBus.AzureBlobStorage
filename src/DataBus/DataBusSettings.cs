namespace NServiceBus.DataBus.AzureBlobStorage
{
    class DataBusSettings
    {
        public DataBusSettings()
        {
            Container = "databus";
            BasePath = "";
            MaxRetries = 5;
            NumberOfIOThreads = 1;
            BackOffInterval = 30; // seconds
            TTL = long.MaxValue; // seconds
            CleanupInterval = 0; // milliseconds, off by default
            ConnectionString = "UseDevelopmentStorage=true";
            ConnectionStringProvided = false;
        }

        public bool ShouldPerformCleanup()
        {
            return CleanupInterval > 0;
        }

        public string Container;
        public int MaxRetries;
        public int BackOffInterval;
        public int NumberOfIOThreads;
        public string BasePath;
        public long TTL;
        public int CleanupInterval;
        public string ConnectionString;
        public bool ConnectionStringProvided;
    }
}