# 1 Hour activation
$Schedule = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedSchedule
$Schedule.Type = "Once"
$Schedule.Duration = "PT1H"
$Schedule.StartDateTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources' -ExternalId "/subscriptions/$($SubscriptionId)"

$RoleDefinition = Get-AzureADMSPrivilegedRoleDefinition `
    -ProviderId 'azureResources' `
    -ResourceId $Resource.Id `
    -Filter "DisplayName eq '$($GroupName)'"

$Account = Get-AzureADUser -Filter "userPrincipalName eq '$($UserEmail)'"

Open-AzureADMSPrivilegedRoleAssignmentRequest `
    -ProviderId 'azureResources' `
    -ResourceId $Resource.Id `
    -RoleDefinitionId $RoleDefinition.Id `
    -SubjectId $Account.ObjectId `
    -Schedule $Schedule `
    -AssignmentState "Active" `
    -Type "UserAdd" `
    -Reason $Reason
