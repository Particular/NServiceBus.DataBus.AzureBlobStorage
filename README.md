# Azure Storage Blob DataBus for NServiceBus

The Azure Storage Blob DataBus for NServiceBus enables the use of the [Azure Storage Blob](https://azure.microsoft.com/en-us/documentation/services/storage/) service as the underlying implementation used by NServiceBus for [DataBus feature](http://docs.particular.net/nservicebus/messaging/databus).

 * [Sample](http://docs.particular.net/samples/azure/blob-storage-databus/)

## How to test locally

To run the tests locally, add a new environment variable `NServiceBus_DataBus_AzureBlobStorage_ConnectionString` containing a connection string to your Azure storage account or the connection string `UseDevelopmentStorage=true` to use the [Azurite emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (ensure it is started before you run the tests).

Additionally, [Microsoft Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer) is an useful free tool that can allow you to view and manage the contents of the Azurite emulator as well as Azure Storage accounts in the cloud.
