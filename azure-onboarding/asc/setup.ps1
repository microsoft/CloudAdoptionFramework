# Login to subscription
$subscription_name = Read-Host "Enter the subscription name secure (Ex: PnP Dev Two)"
az login
az account set -s $subscription_name

# Admin Rol
$ad_group_name_security_admin  = Read-Host "Enter the ad group name to be mapped to security admin role"
$ad_group_object_id = az ad group show --group $ad_group_name_security_admin --query "objectId" --output tsv
az role assignment create --role 'Security Admin' --assignee-object-id $ad_group_object_id
# Reader Rol
$ad_group_name_security_reader = Read-Host "Enter the ad group name to be mapped to security reader role"
$ad_group_object_id = az ad group show --group $ad_group_name_security_reader --query "objectId" --output tsv
az role assignment create --role 'Reader' --assignee-object-id $ad_group_object_id

# This is not really needed, kept just for reference
# az provider register --namespace 'Microsoft.Security'

# Deploys Azure Security Center
$emailSecurityContact = Read-Host "Provide email address for Azure Security Center contact details"
az deployment create --location westus --template-file "templates\deploy-asc.json" --parameters emailSecurityContact=$emailSecurityContact

# Deploys custom policies
Get-ChildItem "policies" -Filter *.json | Foreach-Object {
    az deployment create --location westus --template-file "$($_.FullName)"
}

# Assigns azure security benchmark intiative and custom policy set
az deployment create --location westus --template-file "templates\deploy-policies.json" --parameters emailSecurityContact=$emailSecurityContact

