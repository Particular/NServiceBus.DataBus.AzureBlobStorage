namespace NServiceBus
{
    using System;
    using NServiceBus.ClaimCheck;
    using NServiceBus.ClaimCheck.AzureBlobStorage;

    /// <summary>
    /// Claim check implementation that uses azure blob storage.
    /// </summary>
    public class AzureClaimCheck : ClaimCheckDefinition
    {
        /// <summary>The feature to enable when this databus is selected.</summary>
        protected override Type ProvidedByFeature()
        {
            return typeof(AzureClaimCheckPersistence);
        }
    }
}