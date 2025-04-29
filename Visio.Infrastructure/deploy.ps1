param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "qa", "prod")]
    [string]$Environment,

    [switch]$Destroy
)

$workspace = $Environment
$tfVarsFile = "variables/$Environment.tfvars"

# Check if the tfvars file exists
if (-not (Test-Path $tfVarsFile)) {
    Write-Error "Variable file '$tfVarsFile' does not exist."
    exit 1
}

# Select or create the workspace
Write-Host "Selecting or creating workspace: $workspace"
$workspaceExists = terraform workspace list | Select-String "^\s*\*?\s*$workspace\s*$" -Quiet

if ($workspaceExists) {
    terraform workspace select $workspace
} else {
    terraform workspace new $workspace
}

# Initialize Terraform
Write-Host "Initializing Terraform..."
terraform init

# Apply or Destroy based on flag
if ($Destroy) {
    Write-Host "Destroying infrastructure in '$workspace' using '$tfVarsFile'..."
    terraform destroy -var-file="$tfVarsFile" -auto-approve
} else {
    Write-Host "Applying configuration for '$workspace' using '$tfVarsFile'..."
    terraform apply -var-file="$tfVarsFile" -auto-approve
}
