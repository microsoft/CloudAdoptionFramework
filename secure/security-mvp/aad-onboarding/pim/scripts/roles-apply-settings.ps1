# Available settings can be found at:
# https://github.com/microsoftgraph/microsoft-graph-docs/blob/main/api-reference/beta/api/governancerolesetting-list.md

$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources'

$RoleDefinitions = Get-AzureADMSPrivilegedRoleDefinition -ProviderId 'azureResources' -ResourceId $Resource.Id
$RoleDefinitions = $RoleDefinitions | Where-Object { $_.DisplayName -in $Groups }

foreach($RoleDefinition in $RoleDefinitions) {

    $RoleSettings = Get-AzureADMSPrivilegedRoleSetting -Provider 'azureResources' -Filter "ResourceId eq '$($Resource.Id)' and roleDefinitionId eq '$($RoleDefinition.Id)'"

    $Setting = $RoleSettings.UserMemberSettings | Where-Object { $_.RuleIdentifier -eq 'ExpirationRule' }
    if ($Setting) {
        $Setting.Setting = '{"maximumGrantPeriod":"02:00:00","maximumGrantPeriodInMinutes":120,"permanentAssignment":false}'
    }
    $Setting = $RoleSettings.UserMemberSettings | Where-Object { $_.RuleIdentifier -eq 'MfaRule' }
    if ($Setting) {
        $Setting.Setting = '{"mfaRequired":true}'
    }
    $Setting = $RoleSettings.UserMemberSettings | Where-Object { $_.RuleIdentifier -eq 'JustificationRule' }
    if ($Setting) {
        $Setting.Setting = '{"required":true}'
    }

    Set-AzureADMSPrivilegedRoleSetting `
        -ProviderId 'azureResources' `
        -Id $RoleSettings.Id `
        -ResourceId $Resource.Id  `
        -RoleDefinitionId $RoleDefinition.Id `
        -UserMemberSettings $RoleSettings.UserMemberSettings
}
