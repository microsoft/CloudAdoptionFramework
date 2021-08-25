$templateRootUri="https://raw.githubusercontent.com/pabrus/CloudAdoptionFramework/pabrus/blueprint_to_armdeploy/ready/migration-landing-zone/"

# Login
$tenant_id = Read-Host "Enter the tenant id"
az login --tenant $tenant_id

# Select subscription
$subscription_name = Read-Host "Enter the subscription name"
az account set -s $subscription_name

# Select location
$location = Read-Host "Enter the location (ex: eastus, westus, etc.)"

# Select object id
$object_id = Read-Host "Enter the object ID of a user, service principal or security group in the Azure Active Directory tenant to grant permissions in Key Vault"

# Deploy
az deployment sub create --location $location --template-file .\migration-landing-zone.deploy.json --parameters keyVaultObjectId=$object_id --parameters templateRootUri=$templateRootUri
