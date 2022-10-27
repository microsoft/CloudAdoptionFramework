// Web App - define connection to user GitHub which will create action to update when the user's fork is updated


// Need to figure out how to get image in Linux vs Windows format - dependant on the docker.exe architecture/build

param location string = resourceGroup().location
param FileShareName string
param storageAccountName string

// Storage Account
resource sa 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
  }
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2021-04-01' = {
  name: '${sa.name}/default/${FileShareName}'
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: 'AzNamingTool'
  location: location
}

resource roleAssignmentRG 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().name, managedIdentity.name)
  properties: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions','b24988ac-6180-42a0-ab88-20f7382dd24c')  // Contributor over RG
    principalType: 'ServicePrincipal'
  }
}

output managedIdentityPrincipalId string = managedIdentity.properties.principalId
output managedIdentityName string = managedIdentity.name
output storageAccountResID string = sa.id
output storageAccountAPI string = sa.apiVersion
output fileshareinfo string = fileShare.name
