#pragma warning disable 1591

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