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
            ConnectionString = "UseDevelopmentStorage=true";
            BlockSize = 4 * 1024 * 1024; // Maximum 4MB
            BackOffInterval = 30; // seconds
            TTL = long.MaxValue; // seconds
            CleanupInterval = 300000; // milliseconds
        }

        internal bool ShouldPerformCleanup()
        {
            return CleanupInterval > 0;
        }

        public string ConnectionString;
        public string Container;
        public int MaxRetries;
        public int BackOffInterval;
        public int NumberOfIOThreads;
        public string BasePath;
        public int BlockSize;
        public long TTL;
        public int CleanupInterval;
    }
}