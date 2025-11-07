namespace NServiceBus
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using ClaimCheck.AzureBlobStorage.Config;
    using Features;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.ClaimCheck;
    using NServiceBus.ClaimCheck.AzureBlobStorage;
    using Settings;

    /// <summary>
    /// Claim check implementation that uses azure blob storage.
    /// </summary>
    public class AzureClaimCheck : ClaimCheckDefinition
    {
        internal readonly ClaimCheckSettings ClaimCheckSettings = new();

        /// <inheritdoc />
        protected override void EnableFeature(SettingsHolder settings) => settings.EnableFeature<AzureClaimCheckFeature>();

        class AzureClaimCheckFeature : Feature
        {
            public AzureClaimCheckFeature()
            {
                Enable<Features.ClaimCheck>();

                DependsOn<Features.ClaimCheck>();
            }

            protected override void Setup(FeatureConfigurationContext context)
            {
                var claimCheckSettings = context.Settings.Get<AzureClaimCheck>().ClaimCheckSettings;
                // If a service client has been registered in the container, it will be added later in the configuration process and replace any client set here
                if (claimCheckSettings.CustomBlobServiceProvider is null && claimCheckSettings.ConnectionStringProvided)
                {
                    var blobContainerClient = CreateBlobServiceClient(claimCheckSettings);
                    claimCheckSettings.CustomBlobServiceProvider = new BlobServiceClientProvidedByConfiguration
                    {
                        Client = blobContainerClient
                    };
                }

                context.Services.AddSingleton(claimCheckSettings.CustomBlobServiceProvider ?? new ThrowIfNoBlobServiceClientProvider());
                context.Services.AddSingleton<IClaimCheck>(serviceProvider => new BlobStorageClaimCheck(serviceProvider.GetRequiredService<IProvideBlobServiceClient>(), claimCheckSettings));

                context.Settings.AddStartupDiagnosticsSection(
                    typeof(AzureClaimCheck).FullName,
                    new
                    {
                        ConnectionMechanism = claimCheckSettings.ConnectionStringProvided ? "ConnectionString" : "BlobServiceClient",
                        ContainerName = claimCheckSettings.Container,
                        claimCheckSettings.MaxRetries,
                        claimCheckSettings.BackOffInterval,
                        claimCheckSettings.NumberOfIOThreads,
                    });
            }

            static BlobServiceClient CreateBlobServiceClient(ClaimCheckSettings claimCheckSettings)
            {
                var clientOptions = new BlobClientOptions
                {
                    Retry =
                    {
                        Delay = TimeSpan.FromSeconds(claimCheckSettings.BackOffInterval),
                        MaxRetries = claimCheckSettings.MaxRetries,
                        Mode = RetryMode.Fixed
                    }
                };
                return new BlobServiceClient(claimCheckSettings.ConnectionString, clientOptions);
            }
        }
    }
}