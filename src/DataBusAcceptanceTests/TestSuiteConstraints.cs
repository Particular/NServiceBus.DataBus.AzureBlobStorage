namespace NServiceBus.AcceptanceTests
{
    using AcceptanceTesting.Support;

    public partial class TestSuiteConstraints
    {
        public bool SupportsDtc => false;
        public bool SupportsCrossQueueTransactions => true;
        public bool SupportsNativePubSub => true;
        public bool SupportsNativeDeferral => true;
        public bool SupportsOutbox => true;
        public bool SupportsDelayedDelivery => true;
        public bool SupportsPurgeOnStartup => true;

        public IConfigureEndpointTestExecution CreateTransportConfiguration()
        {
            return new ConfigureEndpointAcceptanceTestingTransport(SupportsNativePubSub, SupportsNativeDeferral);
        }

        public IConfigureEndpointTestExecution CreatePersistenceConfiguration()
        {
            return new ConfigureDatabusEndpoint();
        }
    }
}