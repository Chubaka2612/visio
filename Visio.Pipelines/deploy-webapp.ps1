param (
    [string] $ClientId,
    [string] $ClientSecret,
    [string] $TenantID,
    [string] $SubscriptionId,
    [string] $ResourceGroupName,
    [string] $AppServiceName,
    [string] $ZipFilePath
)

Write-Host "Logging into Azure..."
az login --service-principal -u $ClientId -p $ClientSecret --tenant $TenantID

az account set --subscription $SubscriptionId
Write-Host "Using subscription: $SubscriptionId"

Write-Host "Deploying ZIP to Azure Web App..."
az webapp deployment source config-zip `
   --resource-group $ResourceGroupName `
   --name $AppServiceName `
   --src $ZipFilePath

Write-Host "Deployment completed successfully."
