// Web App - define connection to user GitHub which will create action to update when the user's fork is updated


// Need to figure out how to get image in Linux vs Windows format - dependant on the docker.exe architecture/build

param location string = resourceGroup().location
param storageAccountName string
param storageAccountResId string
param storageAccountAPI string
param appServicePlanName string
// param appRegClientId string
param webSiteName string

// Create Azure Web App
resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'F1'
  }
  kind: 'linux'
}
resource appService 'Microsoft.Web/sites@2020-06-01' = {
  name: webSiteName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v4.0'
      linuxFxVersion: 'DOTNETCORE|6.0'
      ftpsState: 'Disabled'
    }
  }
}

resource appServiceConfigStorage 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'azurestorageaccounts'
  parent: appService
  properties: {
    aznamingtooldata: {
      type: 'AzureFiles'
      accountName: storageAccountName
      shareName: 'aznamingtooldata'
      mountPath: '/app/settings'
      accessKey: listKeys(storageAccountResId, storageAccountAPI).keys[0].value
    }
  }
}

/* REMOVED DUE TO Complexity with adding user to Azure AD role from deployment script
resource appServiceConfigAuth 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'authsettingsV2'
  parent: appService
  properties: {
    globalValidation: {
      requireAuthentication: true
    }
    identityProviders: {
     azureActiveDirectory: {
      enabled: true
      registration: {
        clientId: appRegClientId
      }
     } 
    }
  }
}
*/
