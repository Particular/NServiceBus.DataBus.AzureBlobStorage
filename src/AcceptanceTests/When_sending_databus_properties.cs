using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NUnit.Framework;

public class When_sending_databus_properties
{
    [Test]
    public async Task Should_receive_the_message_the_largeproperty_correctly()
    {
        var endpointConfiguration = new EndpointConfiguration("AzureBlobStorageDataBus.Test");
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.UseTransport<LearningTransport>();
        var environmentVariable = "NServiceBus_DataBus_AzureBlobStorage_ConnectionString";
        var connectionString = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception($"Oh no! We couldn't find an environment variable '{environmentVariable}' with Azure Storage connection string.");
        }
        endpointConfiguration.UseDataBus<AzureDataBus>()
            .ConnectionString(connectionString);

        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.EnableInstallers();

        var endpoint = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        await endpoint.SendLocal(new MyMessageWithLargePayload
        {
            Payload = new DataBusProperty<byte[]>(PayloadToSend)
        }).ConfigureAwait(false);

        ManualResetEvent.WaitOne(TimeSpan.FromSeconds(30));
        await endpoint.Stop().ConfigureAwait(false);
        Assert.AreEqual(PayloadToSend, PayloadReceived, "The large payload should be marshalled correctly using the databus");
    }


    static byte[] PayloadToSend = new byte[1024 * 1024];
    static byte[] PayloadReceived;

    static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

    public class MyMessageHandler : IHandleMessages<MyMessageWithLargePayload>
    {
        public Task Handle(MyMessageWithLargePayload message, IMessageHandlerContext context)
        {
            PayloadReceived = message.Payload.Value;
            ManualResetEvent.Set();
            return Task.FromResult(0);
        }
    }

    public class MyMessageWithLargePayload : ICommand
    {
        public DataBusProperty<byte[]> Payload { get; set; }
    }
}
