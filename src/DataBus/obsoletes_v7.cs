#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Particular.Obsoletes;

    [ObsoleteMetadata(ReplacementTypeOrMember = "NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient", TreatAsErrorFromVersion = "7", RemoveInVersion = "8")]
    [Obsolete("Use 'NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient' instead. Will be removed in version 8.0.0.", true)]
    public interface IProvideBlobServiceClient : ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient
    {
    }
}

namespace NServiceBus
{
    using System;
    using Azure.Storage.Blobs;
    using DataBus;
    using Particular.Obsoletes;

    [ObsoleteMetadata(ReplacementTypeOrMember = "AzureClaimCheck", TreatAsErrorFromVersion = "7", RemoveInVersion = "8")]
    [Obsolete("Use 'AzureClaimCheck' instead. Will be removed in version 8.0.0.", true)]
    public class AzureDataBus : DataBusDefinition
    {
        [ObsoleteMetadata(ReplacementTypeOrMember = "AzureClaimCheck", TreatAsErrorFromVersion = "7", RemoveInVersion = "8")]
        [Obsolete("Use 'AzureClaimCheck' instead. Will be removed in version 8.0.0.", true)]
        protected override Type ProvidedByFeature() => throw new NotImplementedException();
    }

    [ObsoleteMetadata(ReplacementTypeOrMember = "ConfigureAzureClaimCheck", TreatAsErrorFromVersion = "7", RemoveInVersion = "8")]
    [Obsolete("Use 'ConfigureAzureClaimCheck' instead. Will be removed in version 8.0.0.", true)]
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