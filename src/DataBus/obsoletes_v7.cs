#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using Particular.Obsoletes;

    [ObsoleteMetadata(Message = "NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient has been replaced by NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient",
        RemoveInVersion = "8",
        TreatAsErrorFromVersion = "7",
        ReplacementTypeOrMember = "NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient")]
    [Obsolete("NServiceBus.DataBus.AzureBlobStorage.IProvideBlobServiceClient has been replaced by NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient. Use 'NServiceBus.ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient' instead. Will be removed in version 8.0.0.", true)]
    public interface IProvideBlobServiceClient : ClaimCheck.AzureBlobStorage.IProvideBlobServiceClient
    {
    }
}

// Regular Obsolete attributes are needed here because Fody ObsoleteEx attributes are not considered an actual obsolete at compile time.
// The obsolete messages have been duplicated to ensure that the correct message is seen by consumers of the package.
// The ObsoleteEx attributes have been kept to ensure the build error happens when it's time to remove this file.
namespace NServiceBus
{
    using System;
    using Azure.Storage.Blobs;
    using DataBus;
    using Particular.Obsoletes;

    [ObsoleteMetadata(Message = "AzureDataBus has been replaced by AzureClaimCheck",
        RemoveInVersion = "8",
        TreatAsErrorFromVersion = "7",
        ReplacementTypeOrMember = "AzureClaimCheck")]
    [Obsolete("AzureDataBus has been replaced by AzureClaimCheck. Use 'AzureClaimCheck' instead. Will be removed in version 8.0.0.", true)]
    public class AzureDataBus : DataBusDefinition
    {
        [ObsoleteMetadata(Message = "AzureDataBus has been replaced by AzureClaimCheck",
            RemoveInVersion = "8",
            TreatAsErrorFromVersion = "7",
            ReplacementTypeOrMember = "AzureClaimCheck")]
        [Obsolete("AzureDataBus has been replaced by AzureClaimCheck. Use 'AzureClaimCheck' instead. Will be removed in version 8.0.0.", true)]
        protected override Type ProvidedByFeature() => throw new NotImplementedException();
    }

    [ObsoleteMetadata(Message = "AzureDataBus has been replaced by AzureClaimCheck. These extension methods are replaced by the ones on the AzureClaimCheck type",
        RemoveInVersion = "8",
        TreatAsErrorFromVersion = "7",
        ReplacementTypeOrMember = "AzureClaimCheck")]
    [Obsolete("AzureDataBus has been replaced by AzureClaimCheck. These extension methods are replaced by the ones on the AzureClaimCheck type. Use 'AzureClaimCheck' instead. Will be removed in version 8.0.0.", true)]
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