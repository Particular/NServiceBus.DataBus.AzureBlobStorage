namespace NServiceBus
{
    using System;
    using DataBus;
    using NServiceBus.DataBus.AzureBlobStorage;

    /// <summary>
    /// DataBus implementation that uses azure blob storage.
    /// </summary>
    [ObsoleteEx(Message = "AzureDataBus has been replaced by AzureClaimCheck.", RemoveInVersion = "8", TreatAsErrorFromVersion = "7", ReplacementTypeOrMember = "AzureClaimCheck")]
#pragma warning disable CS0618 // Type or member is obsolete
    public class AaaaaaazureDataBus : DataBusDefinition
    {
        /// <summary>The feature to enable when this databus is selected.</summary>
#pragma warning disable CS0672 // Member overrides obsolete member
        protected override Type ProvidedByFeature()
#pragma warning restore CS0672 // Member overrides obsolete member
        {
            return typeof(AzureDataBusPersistence);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}