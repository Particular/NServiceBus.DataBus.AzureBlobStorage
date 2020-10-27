namespace NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTesting.Support;
    using Config;
    using NUnit.Framework;
    
    [TestFixture]
    public class When_setting_blobcontainerclient_through_settings
    {
        [Test]
        public async Task Should_be_registered_in_container()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Endpoint>(builder => builder.When(s => s.SendLocal(new MyMessage())))
                .Done(c => c.Done)
                .Run()
                .ConfigureAwait(false);

            Assert.That(context.BlobContainerClientOfType, Is.EqualTo(typeof(BlobContainerClient)));
        }

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    var environmentVariable = "NServiceBus_DataBus_AzureBlobStorage_ConnectionString";
                    var connectionString = Environment.GetEnvironmentVariable(environmentVariable);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new Exception($"Oh no! We couldn't find an environment variable '{environmentVariable}' with Azure Storage connection string.");
                    }
                    config.UseDataBus<AzureDataBus>()
                        .UseBlobContainerClient(new BlobContainerClient(connectionString, "test"));
                });
            }
        }

        public class DefaultServer : IEndpointSetupTemplate
        {
            public Task<EndpointConfiguration> GetConfiguration(RunDescriptor runDescriptor,
                EndpointCustomizationConfiguration endpointCustomizationConfiguration,
                Action<EndpointConfiguration> configurationBuilderCustomization)
            {
                var configuration = new EndpointConfiguration("AzureBlobStorageDataBus.Test");
                configuration.SendFailedMessagesTo("error");
                configuration.UseTransport<AcceptanceTestingTransport>();
                configuration.UsePersistence<AcceptanceTestingPersistence>();
                configuration.EnableInstallers();
                configuration.TypesToIncludeInScan(new[] {typeof(Context)});

                configurationBuilderCustomization(configuration);

                return Task.FromResult(configuration);
            }
        }

        public class Context : ScenarioContext
        {
            public bool Done { get; set; }
            public Type BlobContainerClientOfType { get; set; }
        }

        public class MyMessage : IMessage
        {
            public string Property { get; set; }
        }

        public class MyMessageHandler : IHandleMessages<MyMessage>
        {
            private readonly IProvideBlobContainerClient blobContainerClientProvider;
            private readonly Context context;

            public MyMessageHandler(Context context, IProvideBlobContainerClient blobContainerClientProvider)
            {
                this.blobContainerClientProvider = blobContainerClientProvider;
                this.context = context;
            }

            public Task Handle(MyMessage message, IMessageHandlerContext handlerContext)
            {
                context.BlobContainerClientOfType = blobContainerClientProvider.Client.GetType();
                context.Done = true;
                return Task.CompletedTask;
            }
        }
    }
}