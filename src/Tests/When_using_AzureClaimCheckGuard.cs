namespace NServiceBus.Azure.Tests.ClaimCheck
{
    using System;
    using NServiceBus.ClaimCheck;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    public class When_using_AzureClaimCheckGuard
    {
        [Test]
        public void Should_not_allow_negative_maximum_retries()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Config.MaxRetries(-1));
        }

        [Test]
        public void Should_not_allow_negative_backoff_interval()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Config.BackOffInterval(-1));
        }

        [Test]
        public void Should_not_allow_invalid_number_of_threads()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Config.NumberOfIOThreads(0));
        }

        [Test]
        public void Should_not_allow_empty_connection_string()
        {
            Assert.Throws<ArgumentException>(() => Config.ConnectionString(string.Empty));
        }

        [Test]
        public void Should_not_allow_null_connection_string()
        {
            Assert.Throws<ArgumentNullException>(() => Config.ConnectionString(null));
        }

        [TestCase("con")]
        [TestCase("co-ntainer")]
        [TestCase("container-name-with-0")]
        [TestCase("6-3-letters-long-container-name-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase("$root")]
        public void Should_allow_valid_container_name(string containerName)
        {
            Config.Container(containerName);
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
            Assert.Throws<ArgumentException>(() => Config.Container(containerName));
        }

        [TestCase(null)]
        [TestCase(" ")]
        public void Should_not_allow_null_or_whitespace_base_path(string basePath)
        {
            Assert.Throws<ArgumentException>(() => Config.BasePath(basePath));
        }

        ClaimCheckExtensions<AzureClaimCheck> Config
        {
            get
            {
                var settings = new SettingsHolder();
                settings.Set(new AzureClaimCheck());
                var extension = new ClaimCheckExtensions<AzureClaimCheck>(settings);

                return extension;
            }
        }
    }
}