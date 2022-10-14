targetScope = 'resourceGroup'

param location string
param currentTime string = utcNow()
param managedIdName string
param managedUserId string
param websiteName string

resource deploymentScript 'Microsoft.Resources/deploymentScripts@2019-10-01-preview' = {
  name: 'ds_createAppReg'
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${resourceId(resourceGroup().name, 'Microsoft.ManagedIdentity/userAssignedIdentities', managedIdName)}': {}
    }
  }
  properties: {
    azPowerShellVersion: '7.0'
    arguments: '-resourceName ${websiteName} -managedUserId ${managedUserId}'
    scriptContent: '''
      param([string] $resourceName, [string] $managedIdentityID)
      connect-azaccount -Identity
      $token = (Get-AzAccessToken -ResourceUrl https://graph.microsoft.com).Token
      $headers = @{'Content-Type' = 'application/json'; 'Authorization' = 'Bearer ' + $token}

      $bodyassign = @{
        "@odata.type" = "#microsoft.graph.unifiedRoleAssignment"
        roleDefinitionId = "9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3"
        principalId = $managedUserId
        directoryScopeId = "/"
      }
      Invoke-RestMethod -Method POST -Headers $headers -Uri  "https://graph.microsoft.com/beta/roleManagement/directory/roleAssignments" -Body ($bodyassign | ConvertTo-Json)

      start-sleep -s 10  # allow time for role assignment to propigate 
      
      $template = @{
        displayName = $resourceName
        requiredResourceAccess = @(
          @{
            resourceAppId = "00000003-0000-0000-c000-000000000000"
            resourceAccess = @(
              @{
                id = "311a71cc-e848-46a1-bdf8-97ff7156d8e6"
                type = "Scope"
              }
            )
          }
        )
        signInAudience = "AzureADMyOrg"
      }
      
      # Upsert App registration
      $app = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/applications?filter=displayName eq '$($resourceName)'").value
      $principal = @{}
      if ($app) {
        $ignore = Invoke-RestMethod -Method Patch -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)" -Body ($template | ConvertTo-Json -Depth 10)
        $principal = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/servicePrincipals?filter=appId eq '$($app.appId)'").value
      } else {
        $app = (Invoke-RestMethod -Method Post -Headers $headers -Uri "https://graph.microsoft.com/beta/applications" -Body ($template | ConvertTo-Json -Depth 10))
        $principal = Invoke-RestMethod -Method POST -Headers $headers -Uri  "https://graph.microsoft.com/beta/servicePrincipals" -Body (@{ "appId" = $app.appId } | ConvertTo-Json)
      }
      
      # Creating client secret
      $app = (Invoke-RestMethod -Method Get -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)")
      
      foreach ($password in $app.passwordCredentials) {
        Write-Host "Deleting secret with id: $($password.keyId)"
        $body = @{
          "keyId" = $password.keyId
        }
        $ignore = Invoke-RestMethod -Method POST -Headers $headers -Uri "https://graph.microsoft.com/beta/applications/$($app.id)/removePassword" -Body ($body | ConvertTo-Json)
      }
      
      $body = @{
        "passwordCredential" = @{
          "displayName"= "Client Secret"
        }
      }
      $secret = (Invoke-RestMethod -Method POST -Headers $headers -Uri  "https://graph.microsoft.com/beta/applications/$($app.id)/addPassword" -Body ($body | ConvertTo-Json)).secretText

      $DeploymentScriptOutputs = @{}
      $DeploymentScriptOutputs['objectId'] = $app.id
      $DeploymentScriptOutputs['clientId'] = $app.appId
      $DeploymentScriptOutputs['clientSecret'] = $secret
      $DeploymentScriptOutputs['principalId'] = $principal.id
      
    '''
    cleanupPreference: 'OnSuccess'
    retentionInterval: 'P1D'
    forceUpdateTag: currentTime // ensures script will run every time
  }
}


output objectId string = deploymentScript.properties.outputs.objectId
output clientId string = deploymentScript.properties.outputs.clientId
output clientSecret string = deploymentScript.properties.outputs.clientSecret
output principalId string = deploymentScript.properties.outputs.principalId
