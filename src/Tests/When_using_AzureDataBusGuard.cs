namespace NServiceBus.Azure.Tests.DataBus
{
    using System;
    using NServiceBus.DataBus;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    public class When_using_AzureDataBusGuard
    {
        [Test]
        public void Should_not_allow_negative_maximum_retries()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => config.MaxRetries(-1));
        }

        [Test]
        public void Should_not_allow_negative_backoff_interval()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => config.BackOffInterval(-1));
        }

        [TestCase(0)]
        [TestCase(ConfigureAzureDataBus.MaxBlockSize + 1)]
        public void Should_not_allow_block_size_more_than_4MB_or_less_than_one_byte(int blockSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => config.BlockSize(blockSize));
        }

        [Test]
        public void Should_not_allow_invalid_number_of_threads()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => config.NumberOfIOThreads(0));
        }

        [TestCase("")]
        [TestCase(null)]
        public void Should_not_allow_invalid_connection_string(string connectionString)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => config.ConnectionString(connectionString));
        }

        [TestCase("con")]
        [TestCase("co-ntainer")]
        [TestCase("container-name-with-0")]
        [TestCase("6-3-letters-long-container-name-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase("$root")]
        public void Should_allow_valid_container_name(string containerName)
        {
            config.Container(containerName);
        }

        [TestCase("")]
        [TestCase("-conta")]
        [TestCase("co--ntainer")]
        [TestCase("Container")]
        [TestCase("container-")]
        [TestCase("co$ntainer")]
        [TestCase("over-63-letters-long-container-name-AAAAAAAAAAAAAAAAAAAAAAAAAAAB")]
        [TestCase(null)]
        public void Should_not_allow_invalid_container_name(string containerName)
        {
            Assert.Throws<ArgumentException>(() => config.Container(containerName));
        }

        [TestCase(null)]
        [TestCase(" ")]
        public void Should_not_allow_null_or_whitespace_base_path(string basePath)
        {
            Assert.Throws<ArgumentException>(() => config.BasePath(basePath));
        }

        [Test]
        public void Should_not_allow_negative_default_time_to_live()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => config.DefaultTTL(-1L));
        }

        DataBusExtentions<AzureDataBus> config = new DataBusExtentions<AzureDataBus>(new SettingsHolder());
    }
}