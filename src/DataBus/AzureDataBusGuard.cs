namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.Text.RegularExpressions;

    class AzureDataBusGuard
    {
        public static void CheckMaxRetries(int maxRetries)
        {
            if (maxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries), maxRetries, "Must be non negative.");
            }
        }

        public static void CheckBackOffInterval(int backOffInterval)
        {
            if (backOffInterval < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(backOffInterval), backOffInterval, "Must not be negative.");
            }
        }

        public static void CheckBlockSize(int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Must not be negative.");
            }
         
            if (blockSize > MaxBlockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, "Must be less than 4mb");
            }
        }

        public static void CheckNumberOfIOThreads(int numberOfIOThreads)
        {
            if (numberOfIOThreads <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfIOThreads), numberOfIOThreads, "Should not be less than one.");
            }
        }

        public static void CheckConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Should not be an empty string.", nameof(connectionString));
            }
        }

        public static void CheckContainerName(string containerName)
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

            throw new ArgumentException(errorMessage, nameof(containerName));
        }

        static bool IsValidBlobContainerName(object containerName)
        {
            return !string.IsNullOrWhiteSpace((string)containerName) &&
                   Regex.IsMatch((string)containerName, @"^(([a-z\d]((-(?=[a-z\d]))|([a-z\d])){2,62})|(\$root))$");
        }

        public static void CheckBasePath(string basePath)
        {
            var value = basePath != null ? basePath : " ";
            var spacesOnly = value.Trim().Length == 0 && value.Length != 0;

            if (spacesOnly)
            {
                throw new ArgumentException("Should not be null or spaces only.", nameof(basePath));
            }
        }

        public static void CheckDefaultTTL(long defaultTTL)
        {
            if (defaultTTL < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultTTL), defaultTTL, "Should not be negative.");
            }
        }

        public const int MaxBlockSize = 4 * 1024 * 1024; //4 mb
    }
}