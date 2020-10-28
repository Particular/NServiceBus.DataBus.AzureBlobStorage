namespace NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Config;

    public class When_custom_provider_registered : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_be_used()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithCustomProvider>(b => b.When(session => session.SendLocal(new DataBusMessage()
                {
                    Payload = new DataBusProperty<byte[]>(new byte[1024 * 1024])
                })))
                .Done(c => c.MessageReceived)
                .Run();

            Assert.True(context.ProviderWasCalled);
        }

        public class Context : ScenarioContext
        {
            public bool MessageReceived { get; set; }
            public bool ProviderWasCalled { get; set; }
        }

        public class EndpointWithCustomProvider : EndpointConfigurationBuilder
        {
            public EndpointWithCustomProvider()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.UseDataBus<AzureDataBus>();
                    config.RegisterComponents(c =>
                        c.AddSingleton<IProvideBlobContainerClient>(provider => new CustomProvider(provider.GetRequiredService<Context>())));
                });
            }

            public class DataBusMessageHandler : IHandleMessages<DataBusMessage>
            {
                public DataBusMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(DataBusMessage message, IMessageHandlerContext context)
                {
                    testContext.MessageReceived = true;
                    return Task.CompletedTask;
                }

                readonly Context testContext;
            }

            public class CustomProvider : IProvideBlobContainerClient
            {
                public CustomProvider(Context testContext)
                {
                    this.testContext = testContext;
                }

                public BlobContainerClient Client
                {
                    get
                    {
                        testContext.ProviderWasCalled = true;
                        return SetupFixture.BlobContainerClient;
                    }
                }

                readonly Context testContext;
            }
        }

        public class DataBusMessage : ICommand
        {
            public DataBusProperty<byte[]> Payload { get; set; }
        }
    }
}