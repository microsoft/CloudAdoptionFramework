// Web App - define connection to user GitHub which will create action to update when the user's fork is updated

targetScope = 'subscription'
// Need to figure out how to get image in Linux vs Windows format - dependant on the docker.exe architecture/build

@minLength(3)
@maxLength(5)
@description('Company Name Identifier. (3-5 characters)')
param companyName string = 'fta'

@description('Location / Region for deployment.')
param location string = deployment().location

@description('File Share Folder Name.')
param FileShareName string = 'aznamingtooldata'

@description('Resource Group Name.')
param ResourceGroupName string

@description('The name of the Storage Account')
param storageAccountName string = 'stor${companyName}${uniqueString(ResourceGroupName)}'

var appServicePlanName = 'appsvcplan-aznamingtool'
var webSiteName = '${companyName}-aznamingtool'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: ResourceGroupName
  location: location
}

module rgManageIdStor './modules/rgManageIdStor.bicep' = {
  scope: resourceGroup
  name: 'mod_${resourceGroup.name}_manageIdStor'
  params: {
    FileShareName: FileShareName
    location: location
    storageAccountName: storageAccountName
  }
}

/*  REMOVED DUE TO Complexities in assigning AD Roles via deployment script
module dsCreateAppReg './modules/ds_createAppReg.bicep' = {
  scope: resourceGroup
  name: 'mod_ds_CreateAppRegistration'
  params: {
    location: location
    managedUserId: rgManageIdStor.outputs.managedIdentityPrincipalId
    managedIdName: rgManageIdStor.outputs.managedIdentityName
    websiteName: webSiteName
  }
}
*/

module rgWebApp './modules/rgWebApp.bicep' = {
  scope: resourceGroup
  name: 'mod_${resourceGroup.name}_WebApp'
  params: {
    appServicePlanName: appServicePlanName
    location: location
    storageAccountName: storageAccountName
    storageAccountResId: rgManageIdStor.outputs.storageAccountResID
    storageAccountAPI: rgManageIdStor.outputs.storageAccountAPI
    // appRegClientId: dsCreateAppReg.outputs.clientId
    webSiteName: webSiteName
  }
}








