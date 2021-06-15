#
# Available settings can be found at:
# https://github.com/microsoftgraph/microsoft-graph-docs/blob/main/api-reference/beta/api/governancerolesetting-list.md
#
$UserMemberSettings = New-Object Collections.Generic.List[Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedRuleSetting]
$Setting = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedRuleSetting
$Setting.RuleIdentifier = "ExpirationRule"
$Setting.Setting = "{'permanentAssignment':false, 'maximumGrantPeriodInMinutes': 120}"
$UserMemberSettings.Add($Setting)

$Setting = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedRuleSetting
$Setting.RuleIdentifier = "MfaRule"
$Setting.Setting = "{'mfaRequired':true}"
$UserMemberSettings.Add($Setting)

$Setting = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedRuleSetting
$Setting.RuleIdentifier = "JustificationRule"
$Setting.Setting = "{'required':true}"
$UserMemberSettings.Add($Setting)

$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources' -ExternalId "/subscriptions/$($SubscriptionId)"

$RoleDefinitions = Get-AzureADMSPrivilegedRoleDefinition -ProviderId 'azureResources' -ResourceId $Resource.Id 
$RoleDefinitions = $RoleDefinitions | Where-Object { $_.DisplayName -in $Groups }

foreach($RoleDefinition in $RoleDefinitions) {

    $RoleSettings = Get-AzureADMSPrivilegedRoleSetting `
        -Provider 'azureResources' `
        -Filter "ResourceId eq '$($Resource.Id)' and roleDefinitionId eq '$($RoleDefinition.Id)'"
    $RoleSettingId = $RoleSettings.Id

    Set-AzureADMSPrivilegedRoleSetting `
        -ProviderId 'azureResources' `
        -Id $RoleSettingId  `
        -ResourceId $Resource.Id  `
        -RoleDefinitionId $RoleDefinition.Id `
        -UserMemberSettings $UserMemberSettings
}
