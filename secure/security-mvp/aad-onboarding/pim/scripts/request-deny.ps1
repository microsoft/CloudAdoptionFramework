Set-AzureADMSPrivilegedRoleAssignmentRequest `
    -ProviderId 'azureResources' `
    -Id $RoleAssignmentRequestId `
    -Decision "AdminDenied"
    -Reason $Reason
