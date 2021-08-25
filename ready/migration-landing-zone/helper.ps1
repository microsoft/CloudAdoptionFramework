$templateRootUri="https://raw.githubusercontent.com/pabrus/CloudAdoptionFramework/pabrus/blueprint_to_armdeploy/ready/migration-landing-zone/"

# # Login
# $tenant_id = Read-Host "Enter the tenant id"
$tenant_id = "72f988bf-86f1-41af-91ab-2d7cd011db47"
az login --tenant $tenant_id

# # Select subscription
# $subscription_name = Read-Host "Enter the subscription name"
$subscription_name = "PnP CAF Security MVP"
az account set -s $subscription_name

# # Select location
# $location = Read-Host "Enter the location (ex: eastus, westus, etc.)"
$location="eastus"

# # Select object if
# $object_id = Read-Host "Enter the object ID of a user, service principal or security group in the Azure Active Directory tenant to grant permissions in Key Vault"
$object_id="71470618-5cc3-43bd-97c1-ab713b9a9212"

# Deploy CMD
az deployment sub create --location $location --template-file .\migration-landing-zone.deploy.json --parameters keyVaultObjectId=$object_id --parameters templateRootUri=$templateRootUri

# Deploy UI
# https://portal.azure.com/#blade/Microsoft_Azure_CreateUIDef/CustomDeploymentBlade/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpabrus%2FCloudAdoptionFramework%2Fpabrus%2Fblueprint_to_armdeploy%2Fready%2Fmigration-landing-zone%2Fmigration-landing-zone.deploy.json/createUIDefinitionUri/https%3A%2F%2Fraw.githubusercontent.com%2Fpabrus%2FCloudAdoptionFramework%2Fpabrus%2Fblueprint_to_armdeploy%2Fready%2Fmigration-landing-zone%2Fmigration-landing-zone.ui.json


# https://portal.azure.com/#blade/Microsoft_Azure_CreateUIDef/CustomDeploymentBlade/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fpabrus%2FCloudAdoptionFramework%2Fpabrus%2Fblueprint_to_armdeploy%2Fready%2Fmigration-landing-zone%2Fmigration-landing-zone.deploy.json

# # DEPLOY SHARED SERVICES
# az deployment sub create --location $location --template-file .\Artifacts\sharedServices.json --parameters keyVaultObjectId=$object_id --parameters templateRootUri=$templateRootUri
# # DEPLOY VNET
# az deployment sub create --location $location --template-file .\Artifacts\vnet.json
# # DEPLOY AZURE MIGRATE
# az deployment sub create --location $location --template-file .\Artifacts\azureMigrate.json
# # DEPLOY AZURE SITE RECOVERY
# az deployment sub create --location $location --template-file .\Artifacts\azureSiteRecovery.json

