# 1 Hour activation
$Schedule = New-Object Microsoft.Open.MSGraph.Model.AzureADMSPrivilegedSchedule
$Schedule.Type = "Once"
$Schedule.Duration = "PT1H"
$Schedule.StartDateTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

Set-AzureADMSPrivilegedRoleAssignmentRequest `
    -ProviderId 'azureResources' `
    -Id $RoleAssignmentRequestId `
    -Decision "AdminApproved"
    -Schedule $Schedule
    -AssignmentState "Active"
