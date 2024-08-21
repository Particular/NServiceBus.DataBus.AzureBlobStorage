namespace NServiceBus.DataBus.AzureBlobStorage
{
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.ClaimCheck.AzureBlobStorage;
    using NServiceBus.Features;

    class AzureDataBusPersistence : AzureClaimCheckPersistence
    {
        public AzureDataBusPersistence()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            DependsOn<DataBus>();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            base.Setup(context);

#pragma warning disable CS0618 // Type or member is obsolete
            context.Services.AddTransient<IDataBus>(serviceProvider => serviceProvider.GetService<BlobStorageClaimCheck>());
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
