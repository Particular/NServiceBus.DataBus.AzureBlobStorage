# NServiceBus.DataBus.AzureBlobStorage

NServiceBus.DataBus.AzureBlobStorage enables the use of the [Azure Storage Blob](https://azure.microsoft.com/en-us/documentation/services/storage/) service as the underlying implementation used by NServiceBus for the [Data Bus feature](https://docs.particular.net/nservicebus/messaging/claimcheck/) to store the payload.

It is part of the [Particular Service Platform](https://particular.net/service-platform), which includes [NServiceBus](https://particular.net/nservicebus) and tools to build, monitor, and debug distributed systems.

See the [Azure Blob Storage Data Bus documentation](https://docs.particular.net/nservicebus/messaging/claimcheck/azure-blob-storage) for more details on how to use it.

## Running tests locally

To run the tests locally, add a new environment variable `NServiceBus_DataBus_AzureBlobStorage_ConnectionString` containing a connection string to your Azure Storage account or the connection string `UseDevelopmentStorage=true` to use the [Azurite emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (ensure it is started before you run the tests).

Additionally, [Microsoft Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer) is a useful free tool that can allow you to view and manage the contents of the Azurite emulator as well as Azure Storage accounts in the cloud.
