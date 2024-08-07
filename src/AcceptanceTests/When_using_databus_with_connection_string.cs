namespace NServiceBus.DataBus.AzureBlobStorage.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.ClaimCheck.DataBus;
    using NUnit.Framework;
    using SystemJsonDataBusSerializer = NServiceBus.ClaimCheck.DataBus.SystemJsonDataBusSerializer;

    public class When_using_databus_with_connection_string : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_work()
        {
            var payloadToSend = new byte[1024 * 1024];
            new Random().NextBytes(payloadToSend);

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithDatabusUsingConnection>(b => b.When(session =>
                    session.SendLocal(new MyMessageWithLargePayload { Payload = new ClaimCheck.DataBus.DataBusProperty<byte[]>(payloadToSend) })))
                .Done(c => c.MessageReceived)
                .Run();

            CollectionAssert.AreEqual(payloadToSend, context.PayloadReceived);
        }

        public class Context : ScenarioContext
        {
            public byte[] PayloadReceived { get; set; }
            public bool MessageReceived { get; set; }
        }

        public class EndpointWithDatabusUsingConnection : EndpointConfigurationBuilder
        {
            public EndpointWithDatabusUsingConnection()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.UseDataBus<AzureDataBus, SystemJsonDataBusSerializer>().ConnectionString(SetupFixture.GetEnvConfiguredConnectionString());
                });
            }

            public class DataBusMessageHandler : IHandleMessages<MyMessageWithLargePayload>
            {
                public DataBusMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyMessageWithLargePayload message, IMessageHandlerContext context)
                {
                    testContext.PayloadReceived = message.Payload.Value;
                    testContext.MessageReceived = true;
                    return Task.CompletedTask;
                }

                readonly Context testContext;
            }
        }

        public class MyMessageWithLargePayload : ICommand
        {
            public ClaimCheck.DataBus.DataBusProperty<byte[]> Payload { get; set; }
        }
    }
}