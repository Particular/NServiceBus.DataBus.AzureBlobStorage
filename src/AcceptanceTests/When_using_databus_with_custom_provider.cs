namespace NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.DependencyInjection;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Config;

    public class When_using_databus_with_custom_provider : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_work()
        {
            var payloadToSend = new byte[1024 * 1024];
            new Random().NextBytes(payloadToSend);

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithCustomProvider>(b => b.When(session => session.SendLocal(new MessageWithLargePayload
                {
                    Payload = new DataBusProperty<byte[]>(payloadToSend)
                })))
                .Done(c => c.MessageReceived)
                .Run();

            Assert.True(context.ProviderWasCalled);
            CollectionAssert.AreEqual(payloadToSend, context.PayloadReceived);
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
                    config.RegisterComponents(services => services.AddSingleton<IProvideBlobServiceClient, TestableBlobServiceClientProvider>());

                    config.UseDataBus<AzureDataBus>();
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

            public class TestableBlobServiceClientProvider  : IProvideBlobServiceClient
            {
                public TestableBlobServiceClientProvider (Context testContext)
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
            public DataBusProperty<byte[]> Payload { get; set; }
        }
    }
}