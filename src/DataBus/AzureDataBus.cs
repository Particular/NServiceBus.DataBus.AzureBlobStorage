namespace NServiceBus
{
    using System;
    using DataBus;

    /// <summary>
    /// DataBus implementation that uses azure blob storage.
    /// </summary>
    public class AzureDataBus : DataBusDefinition
    {
        protected override Type ProvidedByFeature()
        {
            return typeof(AzureDataBusPersistence);
        }
    }
}