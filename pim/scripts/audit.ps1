$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources' -ExternalId "/subscriptions/$($SubscriptionId)"

$RoleDefinitions = Get-AzureADMSPrivilegedRoleDefinition -ProviderId 'azureResources' -ResourceId $Resource.Id | Where-Object { $_.DisplayName -in $Groups }
$RoleAssignments = Get-AzureADMSPrivilegedRoleAssignment -ProviderId 'azureResources' -ResourceId $Resource.Id | Where-Object { $_.RoleDefinitionId -in $RoleDefinitions.Id }

$RoleMembers = foreach ($RoleAssignment in $RoleAssignments) {
    $Account = Get-AzureAdUser -ObjectId $RoleAssignment.SubjectId
    $LastLogon = (Get-AzureAdAuditSigninLogs -top 1 -filter "UserId eq '$($Account)'" | Select-Object CreatedDateTime).CreatedDateTime
    if ($LastLogon) {
        $LastLogon = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId((Get-Date -Date $LastLogon), (Get-TimeZone).Id)
    }

    $Result = New-Object -TypeName psobject
    $Result | Add-Member -MemberType NoteProperty -Name "RoleName" -Value ($RoleDefinitions | Where-Object { $_.Id -eq $RoleAssignments.RoleDefinitionId }).DisplayName
    $Result | Add-Member -MemberType NoteProperty -Name "UserId" -Value $Account.ObjectID
    $Result | Add-Member -MemberType NoteProperty -Name "UserAccount" -Value $Account.DisplayName
    $Result | Add-Member -MemberType NoteProperty -Name "UserPrincipalName" -Value $Account.UserPrincipalName
    $Result | Add-Member -MemberType NoteProperty -Name "AssignmentState" -Value $RoleAssignment.AssignmentState
    $Result | Add-Member -MemberType NoteProperty -Name "LastLogon" -Value $LastLogon
    $Result
}

$RoleMembers | Sort-Object AccountCreated -Descending | Format-Table
