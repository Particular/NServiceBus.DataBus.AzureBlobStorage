namespace NServiceBus.Azure.Tests.DataBus
{
    using System;
    using NServiceBus.DataBus.AzureBlobStorage;
    using NUnit.Framework;
 
    [TestFixture]
    public class When_using_AzureDataBusGuard
    {
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_not_allow_negative_maximum_retries()
        {
            AzureDataBusGuard.CheckMaxRetries(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_not_allow_negative_backoff_interval()
        {
            AzureDataBusGuard.CheckBackOffInterval(-1);
        }

        [TestCase(0)]
        [TestCase(AzureDataBusGuard.MaxBlockSize + 1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_not_allow_block_size_more_than_4MB_or_less_than_one_byte(int blockSize)
        {
            AzureDataBusGuard.CheckBlockSize(blockSize);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_not_allow_invalid_number_of_threads()
        {
            AzureDataBusGuard.CheckNumberOfIOThreads(0);
        }

        [TestCase("")]
        [TestCase(null)]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_not_allow_invalid_connection_string(string connectionString)
        {
            AzureDataBusGuard.CheckConnectionString(connectionString);
        }

        [TestCase("con")]
        [TestCase("co-ntainer")]
        [TestCase("container-name-with-0")]
        [TestCase("6-3-letters-long-container-name-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase("$root")]
        public void Should_allow_valid_container_name(string containerName)
        {
            AzureDataBusGuard.CheckContainerName(containerName);
        }

        [TestCase("")]
        [TestCase("-conta")]
        [TestCase("co--ntainer")]
        [TestCase("Container")]
        [TestCase("container-")]
        [TestCase("co$ntainer")]
        [TestCase("over-63-letters-long-container-name-AAAAAAAAAAAAAAAAAAAAAAAAAAAB")]
        [TestCase(null)]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_not_allow_invalid_container_name(string containerName)
        {
            AzureDataBusGuard.CheckContainerName(containerName);
        }

        [TestCase(null)]
        [TestCase(" ")]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_not_allow_null_or_whitespace_base_path(string basePath)
        {
            AzureDataBusGuard.CheckBasePath(basePath);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Should_not_allow_negative_default_time_to_live()
        {
            AzureDataBusGuard.CheckDefaultTTL(-1L);
        }
    }
}
