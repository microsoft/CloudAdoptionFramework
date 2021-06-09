# Set the schedule for role assignment request
# No end time means a permanent assignment
$Schedule = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedSchedule
$Schedule.Type = "Once"
$Schedule.StartDateTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

$Resource = Get-AzureADMSPrivilegedResource -ProviderId 'azureResources' -ExternalId "/subscriptions/$($SubscriptionId)"

$RoleDefinitions = Get-AzureADMSPrivilegedRoleDefinition -ProviderId 'azureResources' -ResourceId $Resource.Id
$RoleDefinitions = $RoleDefinitions | Where-Object { $_.DisplayName -in $Groups }

foreach($RoleDefinition in $RoleDefinitions) {

    $RoleAssignments = Get-AzureADMSPrivilegedRoleAssignment `
        -ProviderId 'azureResources' `
        -ResourceId $Resource.Id `
        -Filter "roleDefinitionId eq '$($RoleDefinition.Id)' and assignmentState eq 'Active'"

    # We need to remove the break-glass accounts to avoid adding PIM to those
    $RoleAssignments = $RoleAssignments | Where-Object { 
        $RoleAssignmentAccount = Get-AzureAdUser -ObjectId $_.SubjectId
        $RoleAssignmentAccount.UserPrincipalName -notin $BreakGlassAccounts 
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
