# Power Platform Lab Setup Instructions

## Pre-requisites

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Bicep - [Install from PowerShell](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install#azure-powershell)
- [Azure Az PowerShell Module](https://learn.microsoft.com/en-us/powershell/azure/install-az-ps)

## Folder Structure

In this folder, there are two files:

* AzureLabEnvironment.bicep
* Setup-AzureLabEnvironment.ps1

The PowerShell script will run the Bicep template to create the following resources:

- Azure App Service - running .NET 7
- Azure App Service Plan
- Azure Cosmos DB for NoSQL account
- API Management

**Note:** If you have to re-run this script multiple times, note that API Management resources are soft deleted. If you get into this situation, check this site for more details: <https://aka.ms/apimsoftdelete>.

The PowerShell script will also set up environment variables for the API to run locally as well as in Azure. It will deploy the API code to the Azure App Service.

## Deploy the Resources

Before you run the PowerShell script, log into the Azure portal and get your Subscription ID or your Subscription Name. You will need this for PowerShell to connect to your Azure account.

To deploy the resources, run the following command in PowerShell:

```powershell
.\Setup-AzureLabEnvironment.ps1
```

## Run the API locally

If you want to run the API locally, then use the following commands:

```powershell
cd ..\Contoso.Healthcare
dotnet run
```

The output of this command will show you the URL it is running on. By default, it should be `http://localhost:5000`.

## Testing the API with Postman

We have included a [Postman collection](./TAW-PowerApps-Contoso.postman_collection.json) to test the API.

* There is a variable named `urlroot` that is defaulted to `http://localhost:5000`. This is initially set up to test the API locally.
* Once deployed to Azure, you can set the `urlroot` to the `https://YOUR_APP_SERVICE.azurewebsites.net` URL to test the API in Azure.
