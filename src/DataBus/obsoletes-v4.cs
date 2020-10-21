#pragma warning disable 1591
using System;
using NServiceBus.DataBus;

namespace NServiceBus
{
    public static partial class ConfigureAzureDataBus
    {
        [ObsoleteEx(
            Message = "Managed Identity authentication is supported by configuring Azure databus with a BlobContainerClient using the '.UseBlobContainerClient()' configuration API.",
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
            Message = "It's no longer possible to override the blocksize due to restrictions of the underlying SDK",
            RemoveInVersion = "5.0.0",
            TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> BlockSize(this DataBusExtensions<AzureDataBus> config, int blockSize)
        {
            throw new NotImplementedException();
        }
    }
}