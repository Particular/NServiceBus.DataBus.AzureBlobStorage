namespace NServiceBus.ClaimCheck.AzureBlobStorage.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_using_claimcheck_with_custom_provider : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_work()
        {
            var payloadToSend = new byte[1024 * 1024];
            new Random().NextBytes(payloadToSend);

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithCustomProvider>(b => b.When(session => session.SendLocal(new MessageWithLargePayload
                {
                    Payload = new ClaimCheckProperty<byte[]>(payloadToSend)
                })))
                .Done(c => c.MessageReceived)
                .Run();

            Assert.That(context.ProviderWasCalled);
            Assert.That(context.PayloadReceived, Is.EqualTo(payloadToSend).AsCollection);
        }

        public class Context : ScenarioContext
        {
            public byte[] PayloadReceived { get; set; }
            public bool MessageReceived { get; set; }
            public bool ProviderWasCalled { get; set; }
        }

        public class EndpointWithCustomProvider : EndpointConfigurationBuilder
        {
            public EndpointWithCustomProvider()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.RegisterComponents(services => services.AddSingleton<IProvideBlobServiceClient, CustomProvider>());
                });
            }

            public class DataBusMessageHandler : IHandleMessages<MessageWithLargePayload>
            {
                public DataBusMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MessageWithLargePayload messageWithLargePayload, IMessageHandlerContext context)
                {
                    testContext.PayloadReceived = messageWithLargePayload.Payload.Value;
                    testContext.MessageReceived = true;
                    return Task.CompletedTask;
                }

                readonly Context testContext;
            }

            public class CustomProvider : IProvideBlobServiceClient
            {
                public CustomProvider(Context testContext)
                {
                    this.testContext = testContext;
                }

                public BlobServiceClient Client
                {
                    get
                    {
                        testContext.ProviderWasCalled = true;
                        return SetupFixture.BlobServiceClient;
                    }
                }

                readonly Context testContext;
            }
        }

        public class MessageWithLargePayload : ICommand
        {
            public ClaimCheckProperty<byte[]> Payload { get; set; }
        }
    }
}