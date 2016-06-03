namespace NServiceBus
{
    using System;
    using DataBus;

    /// <summary>
    /// DataBus implementation that uses azure blob storage.
    /// </summary>
    public class AzureDataBus : DataBusDefinition
    {
        /// <summary>The feature to enable when this databus is selected.</summary>
        protected override Type ProvidedByFeature()
        {
            return typeof(DataBus.AzureBlobStorage.AzureDataBusPersistence);
        }
    }
}