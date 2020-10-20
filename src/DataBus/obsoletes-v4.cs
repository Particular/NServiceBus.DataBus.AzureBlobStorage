#pragma warning disable 1591
using System;
using NServiceBus.DataBus;

namespace NServiceBus
{
    public static partial class ConfigureAzureDataBus
    {
        [ObsoleteEx(
            Message = "Deprecated because of changes to the unified authentication model. Set a properly setup BlobClientContainer or use ConnectionString instead.",
            RemoveInVersion = "5.0.0",
            TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> AuthenticateWithManagedIdentity(
            this DataBusExtensions<AzureDataBus> config, string storageAccountName,
            TimeSpan renewalTimeBeforeTokenExpires, string endpointSuffix = "core.windows.net")
        {
            throw new NotImplementedException();
        }
        
        [ObsoleteEx(
            Message = "Blocksize has been removed from the new SDK",
            RemoveInVersion = "5.0.0",
            TreatAsErrorFromVersion = "4.0.0")]
        public static DataBusExtensions<AzureDataBus> BlockSize(this DataBusExtensions<AzureDataBus> config, int blockSize)
        {
            throw new NotImplementedException();
        }
    }
}