namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.Text.RegularExpressions;

    class AzureDataBusGuard
    {
        public static void CheckMaxRetries(object maxRetries)
        {
            if ((int) maxRetries < 0)
            {
                throw new ArgumentOutOfRangeException("maxRetries", maxRetries, "MaxRetries should not be negative.");
            }
        }

        public static void CheckBackOffInterval(object backOffInterval)
        {
            if ((int) backOffInterval < 0)
            {
                throw new ArgumentOutOfRangeException("backOffInterval", backOffInterval, "BackOffInterval should not be negative.");
            }
        }

        public static void CheckBlockSize(object blockSize)
        {
            if ((int) blockSize <= 0 || (int) blockSize > AzureDataBusDefaults.DefaultBlockSize)
            {
                throw new ArgumentOutOfRangeException("blockSize", blockSize, "BlockSize should not be negative.");
            }
        }

        public static void CheckNumberOfIOThreads(object numberOfIOThreads)
        {
            if ((int) numberOfIOThreads <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfIOThreads", numberOfIOThreads, "NumberOfIOThreads should not be less than one.");
            }
        }

        public static void CheckConnectionString(object connectionString)
        {
            if (string.IsNullOrWhiteSpace((string) connectionString))
            {
                throw new ArgumentException("ConnectionString should not be an empty string.", "connectionString");
            }
        }

        public static void CheckContainerName(object containerName)
        {
            const string errorMessage =
                "Invalid container name. The container name must be confirming to the following naming rules:" +
                "1. Must start with a letter or number, and can contain only letters, numbers, and the dash (-) character." +
                "2. Every dash (-) character must be immediately preceded and followed by a letter or number." +
                "3. All letters must be lowercase." +
                "4. Container name must be from 3 through 63 characters long.";

            if (IsValidBlobContainerName(containerName))
            {
                return;
            }

            throw new ArgumentException(errorMessage, "containerName");
        }

        static bool IsValidBlobContainerName(object containerName)
        {
            return !string.IsNullOrWhiteSpace((string) containerName) &&
                   Regex.IsMatch((string) containerName, @"^(([a-z\d]((-(?=[a-z\d]))|([a-z\d])){2,62})|(\$root))$");
        }

        public static void CheckBasePath(object basePath)
        {
            var value = basePath != null ? (string) basePath : " ";
            var spacesOnly = value.Trim().Length == 0 && value.Length != 0;

            if (spacesOnly)
            {
                throw new ArgumentException("BasePath name should not be null or spaces only.", "basePath");
            }
        }

        public static void CheckDefaultTTL(object defaultTTL)
        {
            if (defaultTTL.GetType() != typeof(long))
            {
                throw new ArgumentException("defaultTTL should be of type long", "defaultTTL");
            }
            if ((long) defaultTTL < 0)
            {
                throw new ArgumentOutOfRangeException("defaultTTL", defaultTTL, "DefaultTTL should not be negative.");
            }
        }
    }
}