$templateRootUri="https://raw.githubusercontent.com/microsoft/CloudAdoptionFramework/main/secure/security-mvp/deploy"

# Login
$tenant_id = Read-Host "Enter the tenant id"
az login --tenant $tenant_id

# Select subscription
$subscription_name = Read-Host "Enter the subscription name"
az account set -s $subscription_name

# Deploy
$location = Read-Host "Enter the location (ex: eastus, westus, etc.)"
az deployment sub create --location $location --template-file caf-secure-deploy.json --parameters templateRootUri=$templateRootUri
