namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using NServiceBus.ClaimCheck.DataBus;

    /// <summary>
    /// DataBus implementation that uses azure blob storage.
    /// </summary>
    public class AzureDataBus : DataBusDefinition
    {
        /// <summary>The feature to enable when this databus is selected.</summary>
        protected override Type ProvidedByFeature()
        {
            return typeof(AzureDataBusPersistence);
        }
    }
}