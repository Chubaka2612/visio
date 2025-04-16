param(
    [string]$ProjectPath = "..\..\Visio.Web\Visio.Web.csproj",
    [string]$PublishFolderName = "publish",
    [string]$ZipFileName = "publish.zip",
    [string]$ResourceGroup,
    [string]$AppServiceName
)

# Get the root solution folder path
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$solutionRoot = Resolve-Path "$scriptDir\..\.."

# Resolve full paths
$publishFolder = Join-Path $solutionRoot $PublishFolderName
$zipFilePath = Join-Path $solutionRoot $ZipFileName
$resolvedProjectPath = Resolve-Path (Join-Path $scriptDir $ProjectPath)

# Clean previous publish folder and zip if exist
if (Test-Path $publishFolder) {
    Remove-Item -Recurse -Force $publishFolder
}
if (Test-Path $zipFilePath) {
    Remove-Item -Force $zipFilePath
}

# Publish the project
Write-Host "Publishing project: $resolvedProjectPath"
dotnet publish $resolvedProjectPath -c Release -o $publishFolder

# Zip the contents
Write-Host "Creating zip at: $zipFilePath"
Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipFilePath

# Deploy with Azure CLI
Write-Host "Deploying to Azure Web App: $AppServiceName in Resource Group: $ResourceGroup"
az webapp deploy --resource-group $ResourceGroup --name $AppServiceName --src-path $zipFilePath

Write-Host "Deployment complete!"
