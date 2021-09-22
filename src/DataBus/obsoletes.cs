#pragma warning disable 1591
namespace NServiceBus
{
    using System;
    using DataBus;

    public static partial class ConfigureAzureDataBus
    {
        [ObsoleteEx(
            Message = "Managed Identity authentication is supported by configuring Azure databus with a BlobContainerClient using the '.UseBlobServiceClient()' configuration API.",
            ReplacementTypeOrMember = nameof(ConfigureAzureDataBus) + "BlobContainerClient",
            RemoveInVersion = "5.0.0",
            TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> AuthenticateWithManagedIdentity(
            this DataBusExtensions<AzureDataBus> config, string storageAccountName,
            TimeSpan renewalTimeBeforeTokenExpires, string endpointSuffix = "core.windows.net")
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(
            Message = "It's no longer possible to override the blocksize due to restrictions of the underlying SDK.",
            RemoveInVersion = "5.0.0",
            TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> BlockSize(this DataBusExtensions<AzureDataBus> config, int blockSize)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(
             Message = "The default TTL was used for the cleanup mechanism, which has been removed.",
             RemoveInVersion = "5.0.0",
             TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> DefaultTTL(this DataBusExtensions<AzureDataBus> config, long defaultTTLInSeconds)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(
            Message = "The built-in clean-up mechanism for blobs has been removed, please refer to the upgrade guide for alternative options.",
            RemoveInVersion = "5.0.0",
            TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> CleanupInterval(this DataBusExtensions<AzureDataBus> config, int cleanupInterval)
        {
            throw new NotImplementedException();
        }
    }
}