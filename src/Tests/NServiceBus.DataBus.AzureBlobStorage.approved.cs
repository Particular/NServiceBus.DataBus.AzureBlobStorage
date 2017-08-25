﻿[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute(@"NServiceBus.DataBus.AzureBlobStorage.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100dde965e6172e019ac82c2639ffe494dd2e7dd16347c34762a05732b492e110f2e4e2e1b5ef2d85c848ccfb671ee20a47c8d1376276708dc30a90ff1121b647ba3b7259a6bc383b2034938ef0e275b58b920375ac605076178123693c6c4f1331661a62eba28c249386855637780e3ff5f23a6d854700eaa6803ef48907513b92")]
[assembly: System.Runtime.InteropServices.ComVisibleAttribute(false)]
[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.5.2", FrameworkDisplayName=".NET Framework 4.5.2")]

namespace NServiceBus
{
    
    public class AzureDataBus : NServiceBus.DataBus.DataBusDefinition
    {
        public AzureDataBus() { }
        protected override System.Type ProvidedByFeature() { }
    }
    public class static ConfigureAzureDataBus
    {
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> BackOffInterval(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, int backOffInterval) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> BasePath(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, string basePath) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> BlockSize(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, int blockSize) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> ConnectionString(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, string connectionString) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> Container(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, string containerName) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> DefaultTTL(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, long defaultTTLInSeconds) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> MaxRetries(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, int maxRetries) { }
        public static NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> NumberOfIOThreads(this NServiceBus.DataBus.DataBusExtensions<NServiceBus.AzureDataBus> config, int numberOfIOThreads) { }
    }
}