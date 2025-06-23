#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace NServiceBus.DataBus.AzureBlobStorage
{
    [ObsoleteEx(Message = "NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient has been replaced by NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient.",
        RemoveInVersion = "8",
        TreatAsErrorFromVersion = "7",
        ReplacementTypeOrMember = "NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient")]
    public interface IProvideBlobServiceClient : ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient
    {
    }
}

namespace NServiceBus
{
    using System;
    using Azure.Storage.Blobs;
    using DataBus;

    [ObsoleteEx(Message = "AzureDataBus has been replaced by AzureClaimCheck.",
        RemoveInVersion = "8",
        TreatAsErrorFromVersion = "7",
        ReplacementTypeOrMember = "AzureClaimCheck")]
    public class AzureDataBus : DataBusDefinition
    {
        [DoNotWarnAboutObsoleteUsage]
        [Obsolete("AzureDataBus has been replaced by AzureClaimCheck.", true)]
        protected override Type ProvidedByFeature() => throw new NotImplementedException();
    }

    [ObsoleteEx(Message = "AzureDataBus has been replaced by AzureClaimCheck. These extension methods are replaced by the ones on the AzureClaimCheck type.",
        RemoveInVersion = "8",
        TreatAsErrorFromVersion = "7",
        ReplacementTypeOrMember = "AzureClaimCheck")]
    public static class ConfigureAzureDataBus
    {
        public static DataBusExtensions<AzureDataBus> MaxRetries(this DataBusExtensions<AzureDataBus> config, int maxRetries) => throw new NotImplementedException();

        public static DataBusExtensions<AzureDataBus> BackOffInterval(this DataBusExtensions<AzureDataBus> config, int backOffInterval) => throw new NotImplementedException();

        public static DataBusExtensions<AzureDataBus> NumberOfIOThreads(this DataBusExtensions<AzureDataBus> config, int numberOfIOThreads) => throw new NotImplementedException();

        public static DataBusExtensions<AzureDataBus> ConnectionString(this DataBusExtensions<AzureDataBus> config, string connectionString) => throw new NotImplementedException();

        public static DataBusExtensions<AzureDataBus> Container(this DataBusExtensions<AzureDataBus> config, string containerName) => throw new NotImplementedException();

        public static DataBusExtensions<AzureDataBus> BasePath(this DataBusExtensions<AzureDataBus> config, string basePath) => throw new NotImplementedException();


        public static DataBusExtensions<AzureDataBus> UseBlobServiceClient(this DataBusExtensions<AzureDataBus> config, BlobServiceClient blobServiceClient) => throw new NotImplementedException();
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member