namespace NServiceBus
{
    using System;
    using Azure.Core;
    using Azure.Storage.Blobs;
    using ClaimCheck.AzureBlobStorage.Config;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.ClaimCheck;
    using NServiceBus.ClaimCheck.AzureBlobStorage;

    /// <summary>
    /// Claim check implementation that uses azure blob storage.
    /// </summary>
    public class AzureClaimCheck : ClaimCheckDefinition
    {
        internal ClaimCheckSettings ClaimCheckSettings = new();

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            // If a service client has been registered in the container, it will added later in the configuration process and replace any client set here
            if (ClaimCheckSettings.CustomBlobServiceProvider is null && ClaimCheckSettings.ConnectionStringProvided)
            {
                var blobContainerClient = CreateBlobServiceClient();
                ClaimCheckSettings.CustomBlobServiceProvider = new BlobServiceClientProvidedByConfiguration
                {
                    Client = blobContainerClient
                };
            }

            services.AddSingleton(ClaimCheckSettings.CustomBlobServiceProvider ?? new ThrowIfNoBlobServiceClientProvider());
            services.AddSingleton<IClaimCheck>(serviceProvider => new BlobStorageClaimCheck(serviceProvider.GetRequiredService<IProvideBlobServiceClient>(), ClaimCheckSettings));
        }

        BlobServiceClient CreateBlobServiceClient()
        {
            var clientOptions = new BlobClientOptions
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(ClaimCheckSettings.BackOffInterval),
                    MaxRetries = ClaimCheckSettings.MaxRetries,
                    Mode = RetryMode.Fixed
                }
            };
            return new BlobServiceClient(ClaimCheckSettings.ConnectionString, clientOptions);
        }
    }
}