﻿namespace NServiceBus.ClaimCheck.AzureBlobStorage.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_using_claimcheck_with_expiry : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_work()
        {
            var payloadToSend = new byte[1024 * 1024];
            new Random().NextBytes(payloadToSend);

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointReceivingMessageWithExpiry>(b => b.When(session =>
                    session.SendLocal(new MyMessageWithLargePayloadAndExpiry
                    {
                        Payload = new ClaimCheckProperty<byte[]>(payloadToSend)
                    })))
                .Done(c => c.MessageReceived)
                .Run();

            Assert.That(context.PayloadReceived, Is.EqualTo(payloadToSend).AsCollection);
        }

        public class Context : ScenarioContext
        {
            public byte[] PayloadReceived { get; set; }
            public bool MessageReceived { get; set; }
        }

        public class EndpointReceivingMessageWithExpiry : EndpointConfigurationBuilder
        {
            public EndpointReceivingMessageWithExpiry()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    config.UseClaimCheck<AzureClaimCheck, SystemJsonClaimCheckSerializer>().UseBlobServiceClient(SetupFixture.BlobServiceClient);
                });
            }

            public class DataBusMessageHandler : IHandleMessages<MyMessageWithLargePayloadAndExpiry>
            {
                public DataBusMessageHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyMessageWithLargePayloadAndExpiry message, IMessageHandlerContext context)
                {
                    testContext.PayloadReceived = message.Payload.Value;
                    testContext.MessageReceived = true;
                    return Task.CompletedTask;
                }

                readonly Context testContext;
            }
        }

        // Discard after thirty seconds
        [TimeToBeReceived("00:00:30")]
        public class MyMessageWithLargePayloadAndExpiry : ICommand
        {
            public ClaimCheckProperty<byte[]> Payload { get; set; }
        }
    }
}
