$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources'

$RoleDefinitions = Get-AzureADMSPrivilegedRoleDefinition -ProviderId 'azureResources' -ResourceId $Resource.Id | Where-Object { $_.DisplayName -in $Groups }
$RoleAssignments = Get-AzureADMSPrivilegedRoleAssignment -ProviderId 'azureResources' -ResourceId $Resource.Id | Where-Object { $_.RoleDefinitionId -in $RoleDefinitions.Id }

$RoleMembers = foreach ($RoleAssignment in $RoleAssignments) {
    $Result = New-Object -TypeName psobject

    try { $Account = Get-AzureAdUser -ObjectId $RoleAssignment.SubjectId } catch { $Account = $null }
    if ($Account) {
        $LastLogon = (Get-AzureAdAuditSigninLogs -top 1 -filter "UserId eq '$($Account.ObjectId)'" | Select-Object CreatedDateTime).CreatedDateTime
        if ($LastLogon) {
            $LastLogon = [System.TimeZoneInfo]::ConvertTimeBySystemTimeZoneId((Get-Date -Date $LastLogon), (Get-TimeZone).Id)
        }

        $Result | Add-Member -MemberType NoteProperty -Name "RoleName" -Value ($RoleDefinitions | Where-Object { $_.Id -eq $RoleAssignment.RoleDefinitionId }).DisplayName
        $Result | Add-Member -MemberType NoteProperty -Name "UserId" -Value $Account.ObjectId
        $Result | Add-Member -MemberType NoteProperty -Name "UserAccount" -Value $Account.DisplayName
        $Result | Add-Member -MemberType NoteProperty -Name "UserPrincipalName" -Value $Account.UserPrincipalName
        $Result | Add-Member -MemberType NoteProperty -Name "AssignmentState" -Value $RoleAssignment.AssignmentState
        $Result | Add-Member -MemberType NoteProperty -Name "LastLogon" -Value $LastLogon
    }

    $Result
}

$RoleMembers | Sort-Object AccountCreated -Descending | Format-Table
