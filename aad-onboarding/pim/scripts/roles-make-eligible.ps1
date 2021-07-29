# Set the schedule for role assignment request
# Max end date time for eligible assignments is 1 year
$Schedule = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedSchedule
$Schedule.Type = "Once"
$Schedule.StartDateTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
$schedule.endDateTime = $Schedule.StartDateTime.AddYears(1)

$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources'

$RoleDefinitions = Get-AzureADMSPrivilegedRoleDefinition -ProviderId 'azureResources' -ResourceId $Resource.Id
$RoleDefinitions = $RoleDefinitions | Where-Object { $_.DisplayName -in $Groups }

foreach($RoleDefinition in $RoleDefinitions) {

    $RoleAssignments = Get-AzureADMSPrivilegedRoleAssignment `
        -ProviderId 'azureResources' `
        -ResourceId $Resource.Id `
        -Filter "roleDefinitionId eq '$($RoleDefinition.Id)' and assignmentState eq 'Active'"

    # We need to remove the break-glass accounts to avoid adding PIM to those
    $BreakGlassGroup = Get-AzureADGroup -Filter "DisplayName eq 'Emergency Access Accounts'"
    if ($BreakGlassGroup)
    {
        $BreakGlassAccounts = (Get-AzureADGroupMember -ObjectId $BreakGlassGroup.ObjectId).UserPrincipalName
        if ($BreakGlassAccounts) {
            $RoleAssignments = $RoleAssignments | Where-Object {
                try { $RoleAssignmentAccount = Get-AzureAdUser -ObjectId $_.SubjectId } catch { $RoleAssignmentAccount = $null }
                (!$RoleAssignmentAccount) -or ($RoleAssignmentAccount.UserPrincipalName -notin $BreakGlassAccounts)
            }
        }
    }

    # Change all the role's active assignments to eligible
    foreach ($RoleAssignment in $RoleAssignments) {
        Open-AzureADMSPrivilegedRoleAssignmentRequest `
            -ProviderId 'azureResources' `
            -ResourceId $Resource.Id `
            -RoleDefinitionId $RoleDefinition.Id `
            -SubjectId $RoleAssignment.SubjectId `
            -Type "AdminAdd" `
            -AssignmentState "Eligible" `
            -schedule $Schedule `
            -reason "Enable PIM by adding the role assignment to Eligible."
    }
}
