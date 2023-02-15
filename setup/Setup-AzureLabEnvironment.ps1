# 
# Script to setup lab environment for the Power Apps training session
# 

# 
# Script will gather some basic information, log in to the trainee's Azure environment, and utilize Bicep template(s) to build out the Azure lab environment.
# 

# For this script to work, Bicep will need to be installed. 
# As we are running this in a PowerShell script, have them follow these steps if Bicep isn't installed:
# https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install#azure-powershell

# 
# Set script variables
# 
$BicepFilePath = ".\AzureLabEnvironment.bicep"
$Workload = "power-plat"
$Environment = "train"

# Install\import the Az PowerShell module
if (Get-Module -Name Az -ListAvailable) {
    Write-Host "The Az module is installed. Importing now..."
    Import-Module -Name Az
    Write-Host "The Az module is now imported."
} else {
    Write-Host "The Az module is not installed. Installing..."
    Install-Module -Name Az -Force -AllowClobber
    Import-Module -Name Az
    Write-Host "The Az module is now installed."
}

# Get information from training user
Write-Host "First, we will gather some information to prep for resource deployment."
Start-Sleep -Seconds 2
$AzureSubscription = Read-Host "Input the Subscription ID or Subscription Name of the Azure subscription to use"
Start-Sleep -Seconds 2

# Log in to Azure Cloud and set the default subscription to use for deploying training resources
    try {
        Write-Host "A new window in your web browser will open so that you may log in to your Microsoft account. Once logged in, PowerShell will be connected to Azure."
        Start-Sleep -Seconds 5
        Connect-AzAccount -Environment AzureCloud -Subscription $AzureSubscription
        Start-Sleep -Seconds 2
    }
    catch {
        "We were unable to login to Azure."
        Write-Error $_
    }

# Get the region for deployment
Read-Host "You will need to enter a Location for hosting your resources. Press any key to see the list"
Get-AzLocation | Select-Object DisplayName, Location | Sort-Object Location
$AzureResourcesLocation = Read-Host "Input the Azure region (Location) where you would like to deploy resources. Do not use spaces. (eastus, westeurope, etc)"
Start-Sleep -Seconds 2

# Get information to deploy Bicep template
Write-Host "Next, we will gather some information to configure the Azure resources that will be deployed."
Start-Sleep -Seconds 2

Write-Host "You will need to enter a suffix that can be used to create a unique name for each Azure resource."
Write-Host "This unique suffix will be used in naming resources that will created as part of your deployment, such as your initials followed by the current date in YYYYMMDD format (ex. skg20230103)."
Start-Sleep -Seconds 2
$UniqueSuffix = Read-Host "Resource Suffix"
$TrainingResourceGroup = "rg-$Workload-$Environment-$UniqueSuffix"
$AzureResourcesDeploymentName = "$Workload-$Environment-$AzureResourcesLocation-$UniqueSuffix"
$PublisherEmail = Read-Host "Enter your e-mail address. API Management uses this as part of its publisher properties"
$PublisherName = Read-Host "Enter your organization's name. API Management uses this as part of its publisher properties"

# Build Bicep parameter object for Bicep deployment parameters
$BicepParameters = @{
    uniqueSuffix = $UniqueSuffix;
    workload = $Workload;
    environment = $Environment;
    region = $AzureResourcesLocation;
    publisherEmail = $PublisherEmail;
    publisherName = $PublisherName
}

# Set resource names for PowerShell commands below
$CosmosDBAccountName = "cosmos-$Workload-$Environment-$UniqueSuffix"
$CosmosDBName = "HealthCheckDB"
$CosmosDBContainerName = "HealthCheck"
# $ApiManagementServiceName = "apim-$Workload-$Environment-$UniqueSuffix"
# $AppServicePlanName = "plan-$Workload-$Environment-$AzureResourcesLocation-$UniqueSuffix"
$AppServiceName = "app-$Workload-$Environment-$AzureResourcesLocation-$UniqueSuffix"

Write-Host "Thank you! Now deploying these resources..."
Start-Sleep -Seconds 2

# Build tsi-win-with-app-platform resource group
    try {
        Write-Host "Building resource group [$TrainingResourceGroup] to deploy training resources..."
        Start-Sleep -Seconds 2
        New-AzResourceGroup -Name $TrainingResourceGroup -Location $AzureResourcesLocation -Force
        Write-Host "Resource group [$TrainingResourceGroup] deployed."
        Start-Sleep -Seconds 2
    }
    catch {
        "We were unable to create the [$TrainingResourceGroup] resource group."
        Write-Error $_
    }

# Deploy Azure resources in to resource group
    try {
        Write-Host "Deploying resources to [$TrainingResourceGroup]..."
        Start-Sleep -Seconds 2

        New-AzResourceGroupDeployment `
        -ResourceGroupName $TrainingResourceGroup `
        -Name $AzureResourcesDeploymentName `
        -Mode Complete `
        -TemplateFile $BicepFilePath `
        -TemplateParameterObject $BicepParameters `
        -Force

        Write-Host "Resources deployed to [$TrainingResourceGroup]."
        Start-Sleep -Seconds 2
    }
    catch {
        "We were unable to deploy the training resources to the [$TrainingResourceGroup] resource group."
        Write-Error $_
    }

# Change working directory to dotnet app and restore\build the dotnet project
Write-Information "Building the API code..."
Set-Location -Path "..\Contoso.Healthcare"
dotnet restore
dotnet build --configuration Release

# Get the contents of the appsettings.json file and convert it to a PowerShell object from JSON
$AppSettings = Get-Content -Path ".\appsettings.json" -Raw | ConvertFrom-Json

# Get the CosmosDB URL and Primary Master Key values from the CosmosDB Azure resource
$CosmosDB = Get-AzCosmosDBAccount -ResourceGroupName $TrainingResourceGroup -Name $CosmosDBAccountName
$CosmosDBUri = $CosmosDB.DocumentEndpoint

$CosmosDBAccountKey = Get-AzCosmosDBAccountKey -ResourceGroupName $TrainingResourceGroup -Name $CosmosDBAccountName
$CosmosDBAccountKeyPrimaryMasterKey = $CosmosDBAccountKey.PrimaryMasterKey

# Update the Appsettings PowerShell object with the values from the CosmosDB Azure resource
$AppSettings.CosmosDb.Account = $CosmosDBUri
$AppSettings.CosmosDb.Key = $CosmosDBAccountKeyPrimaryMasterKey

# Convert the PowerShell object to JSON format and overwrite the appsettings.json file with the new data from the CosmosDB Azure resource
$AppSettingsJson = ConvertTo-Json -InputObject $AppSettings
Out-File -InputObject $AppSettingsJson -FilePath ".\appsettings.json"

# Update current environment variables with the values for the CosmosDB Azure resource
# These are used if you are running the API locally
$env:PowerAppsLabAccount = "$CosmosDBUri";
$env:PowerAppsLabKey = "$CosmosDBAccountKeyPrimaryMasterKey";
$env:PowerAppsLabDatabaseName = "$CosmosDBName";
$env:PowerAppsLabContainerName = "$CosmosDBContainerName"

# Change App Service Appsettings inside of Azure
Write-Information "Setting the environment variables on Azure App Service..."
$AzureAppSettings = @{
    PowerAppsLabAccount = "$CosmosDBUri";
    PowerAppsLabKey = "$CosmosDBAccountKeyPrimaryMasterKey";
    PowerAppsLabDatabaseName = "$CosmosDBName";
    PowerAppsLabContainerName = "$CosmosDBContainerName"
}
Set-AzWebApp -ResourceGroupName $TrainingResourceGroup -Name $AppServiceName -AppSettings $AzureAppSettings

# Zip application folder
Write-Information "Compressing the API to a ZIP file for deployment..."
$ZipFile = "Contoso.Healthcare.zip"
Set-Location ..
Compress-Archive -Path ".\Contoso.Healthcare\bin\Release\net7.0\*" -DestinationPath ".\Contoso.Healthcare.zip" -Update

# Deploy application to App Service
Write-Information "Deploying the API to Azure App Service..."
Publish-AzWebApp -ResourceGroupName $TrainingResourceGroup -Name $AppServiceName -ArchivePath ".\$ZipFile" -Force

Write-Information "The training's pre-deployment and initial configuration of Azure resources is now complete."
