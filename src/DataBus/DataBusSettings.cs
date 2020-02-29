namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;

    class DataBusSettings
    {
        public DataBusSettings()
        {
            Container = "databus";
            BasePath = "";
            MaxRetries = 5;
            NumberOfIOThreads = 1;
            BlockSize = 4 * 1024 * 1024; // Maximum 4MB
            BackOffInterval = 30; // seconds
            TTL = long.MaxValue; // seconds
            CleanupInterval = 0; // milliseconds, off by default
            RenewalTimeBeforeTokenExpires = TimeSpan.FromMinutes(5);
            StorageAccountName = null;
            ConnectionString = "UseDevelopmentStorage=true";
            UserProvidedConnectionString = false;
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
        public int BlockSize;
        public long TTL;
        public int CleanupInterval;
        public TimeSpan RenewalTimeBeforeTokenExpires;
        public string StorageAccountName;
        public string ConnectionString;
        public bool UserProvidedConnectionString;
    }
}