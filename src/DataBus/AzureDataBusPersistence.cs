namespace NServiceBus.DataBus.AzureBlobStorage
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    class AzureDataBusPersistence : Feature
    {
        public AzureDataBusPersistence()
        {
            DependsOn<DataBus>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var dataBusSettings = context.Settings.GetOrDefault<DataBusSettings>() ?? new DataBusSettings();

            ThrowIfConnectionStringAndTokenProviderSpecified(dataBusSettings);

            var container = CreateCloudBlobContainer(dataBusSettings);

            var dataBus = new BlobStorageDataBus(container, dataBusSettings, new AsyncTimer());

            context.Container.ConfigureComponent(b => dataBus, DependencyLifecycle.SingleInstance);
        }

        static void ThrowIfConnectionStringAndTokenProviderSpecified(DataBusSettings dataBusSettings)
        {
            if (dataBusSettings.StorageAccountName != null && dataBusSettings.UserProvidedConnectionString)
            {
                throw new Exception("More than one authentication method to Azure Service was supplied (using connection string and Managed Identity). Use one method only.");
            }
        }

        static CloudBlobContainer CreateCloudBlobContainer(DataBusSettings dataBusSettings)
        {
            CloudBlobContainer container;

            // Attempt managed identity identity first
            if (!string.IsNullOrWhiteSpace(dataBusSettings.StorageAccountName))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var state = (azureServiceTokenProvider, dataBusSettings);
                var tokenAndFrequency = TokenRenewerAsync(state, CancellationToken.None).GetAwaiter().GetResult();
                var tokenCredential = new TokenCredential(tokenAndFrequency.Token, TokenRenewerAsync, state, tokenAndFrequency.Frequency.Value);
                var storageCredentials = new StorageCredentials(tokenCredential);
                var containerPath = $"https://{dataBusSettings.StorageAccountName}.blob.{dataBusSettings.EndpointSuffix}/{dataBusSettings.Container}";

                container = new CloudBlobContainer(new StorageUri(new Uri(containerPath)), storageCredentials);
            }
            else // fallback to connection string
            {
                var cloudBlobClient = CloudStorageAccount.Parse(dataBusSettings.ConnectionString).CreateCloudBlobClient();
                container = cloudBlobClient.GetContainerReference(dataBusSettings.Container);
            }

            return container;
        }

        static async Task<NewTokenAndFrequency> TokenRenewerAsync(object state, CancellationToken token = default)
        {
            var (azureServiceTokenProvider, settings) = (ValueTuple<AzureServiceTokenProvider, DataBusSettings>)state;

            // Use the same token provider to request a new token.
            var resourceUri = $"https://{settings.StorageAccountName}.blob.{settings.EndpointSuffix}";
            var result = await azureServiceTokenProvider.GetAuthenticationResultAsync(resourceUri, cancellationToken: token).ConfigureAwait(false);

            // Renew the token before it expires.
            var next = result.ExpiresOn - DateTimeOffset.UtcNow - settings.RenewalTimeBeforeTokenExpires;
            if (next.Ticks < 0)
            {
                next = default;
            }

            return new NewTokenAndFrequency(result.AccessToken, next);
        }
    }
}