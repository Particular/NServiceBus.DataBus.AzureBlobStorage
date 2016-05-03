namespace NServiceBus
{
    using Features;

    [ObsoleteEx(
        ReplacementTypeOrMember = "UseDataBusExtensions.UseDataBus<AzureDataBus>()",
        Message = "Use `configuration.UseDataBus<AzureDataBus>()`, where `configuration` is an instance of `EndpointConfiguration`. If self-hosting the instance can be obtained from `new EndpointConfiguration()`. if using the NServiceBus Host the instance of `EndpointConfiguration` will be passed in via the `INeedInitialization` or `IConfigureThisEndpoint` interfaces.",
        RemoveInVersion = "2.0",
        TreatAsErrorFromVersion = "1.0")]
    public class AzureDataBusPersistence : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace NServiceBus
{
    [ObsoleteEx(
       Message = "Default can be found in our documentation.",
        RemoveInVersion = "2.0",
        TreatAsErrorFromVersion = "1.0")]
    public class AzureDataBusDefaults
    {
    }
}

namespace NServiceBus.DataBus.Azure.BlobStorage
{

    [ObsoleteEx(
     Message = "No longer a public api.",
        RemoveInVersion = "2.0",
        TreatAsErrorFromVersion = "1.0")]
    public class BlobStorageDataBus
    {

    }
}

namespace NServiceBus.Config
{
    using System.Configuration;


    [ObsoleteEx(
        Message = "Configuration api no longer supported. Please use the code first api.",
        RemoveInVersion = "2.0",
        TreatAsErrorFromVersion = "1.0")]
    public class AzureDataBusConfig : ConfigurationSection
    {
        public int MaxRetries
        {
            get { return (int)this["MaxRetries"]; }
            set { this["MaxRetries"] = value; }
        }

        public int BackOffInterval
        {
            get { return (int)this["BackOffInterval"]; }
            set { this["BackOffInterval"] = value; }
        }

        public int BlockSize
        {
            get { return (int)this["BlockSize"]; }
            set { this["BlockSize"] = value; }
        }

        public int NumberOfIOThreads
        {
            get { return (int)this["NumberOfIOThreads"]; }
            set { this["NumberOfIOThreads"] = value; }
        }

        public string ConnectionString
        {
            get { return (string)this["ConnectionString"]; }
            set { this["ConnectionString"] = value; }
        }

        public string Container
        {
            get { return (string)this["Container"]; }
            set { this["Container"] = value; }
        }

        public string BasePath
        {
            get { return (string)this["BasePath"]; }
            set { this["BasePath"] = value; }
        }

        public long DefaultTTL
        {
            get { return (long)this["DefaultTTL"]; }
            set { this["DefaultTTL"] = value; }
        }
    }
}