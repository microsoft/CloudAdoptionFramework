Import-Module AzureAD

function createEmergencyAccessAzureADUser([string] $displayName, [securestring] $password)
{
    $upnPrefix = "ea_$($displayName.ToLower())"
    $userPrincipalName = "$($upnPrefix)@$((Get-AzureADCurrentSessionInfo).TenantDomain)"

    $user = Get-AzureADUser -Filter "userPrincipalName eq '$($userPrincipalName)'"

    if (!$user)
    {
        $PasswordProfile = New-Object -TypeName Microsoft.Open.AzureAD.Model.PasswordProfile
        $PasswordProfile.Password = $password
        $PasswordProfile.ForceChangePasswordNextLogin = $false
        $PasswordProfile.EnforceChangePasswordPolicy = $false
        
        $user = New-AzureADUser -AccountEnabled $true -DisplayName "Emergency Access Account $($displayName)" -Department "Information Technology" -PasswordPolicies "DisablePasswordExpiration" -PasswordProfile $PasswordProfile -ShowInAddressList $false -UserPrincipalName $userPrincipalName -UserType "Member" -MailNickName $upnPrefix

        Remove-Variable PasswordProfile        
    }

    return $user
}

function assignAzureADUserToAzureADGroupAndMakeOwner([string] $groupObjectId, [string] $userPrincipalName)
{
    $group = Get-AzureADGroup -ObjectId $groupObjectId
    $user = Get-AzureADUser -ObjectId $userPrincipalName
    
    if ((Get-AzureADGroupMember -ObjectId $group.ObjectId).UserPrincipalName -notcontains $user.userPrincipalName)
    {
        Add-AzureADGroupMember -ObjectId $groupObjectId -RefObjectId $user.ObjectId
    }

    if ((Get-AzureADGroupOwner -ObjectId $group.ObjectId).UserPrincipalName -notcontains $user.userPrincipalName)
    {
        Add-AzureADGroupOwner -ObjectId $groupObjectId -RefObjectId $user.ObjectId
    }
}

function assignEmergencyAccessAzureADUserToAzureADGlobalAdministratorRole([string] $userPrincipalName)
{
     $role = Get-AzureADDirectoryRole | Where-Object {$_.displayName -eq 'Global Administrator'}
     if (!$role)
     {
          $RoleTemplate = Get-AzureADDirectoryRoleTemplate | Where-Object {$_.DisplayName -eq 'Global Administrator'}
          $role = Enable-AzureADDirectoryRole -RoleTemplateId $RoleTemplate.ObjectId
     }
 
     if ((Get-AzureADDirectoryRoleMember -ObjectId $role.ObjectId).UserPrincipalName -notcontains $userPrincipalName)
     {
          Add-AzureADDirectoryRoleMember -ObjectId $role.ObjectId -RefObjectId (Get-AzureADUser -ObjectId $userPrincipalName).ObjectID
     }
}

function createEmergencyAccessAzureAdSecurityGroup()
{
    $group = Get-AzureADGroup -Filter "DisplayName eq 'Emergency Access Accounts'"
    if (!$group)
    {
        $group = New-AzureADGroup -Description 'Exclusively contains this tenant''s emergency access accounts. This group might be excluded from key conditional access policies applied to the rest of the organization. Never add accounts to this group, unless they are designated as emergency access accounts.' -DisplayName 'Emergency Access Accounts' -MailEnabled $false -SecurityEnabled $true -MailNickName 'emergencyAccessAccounts'
    }
    return $group
}

function provisionEmergencyAccessAzureADUser([string] $displayName, [securestring] $password, [string] $emergencyAccessAzureAdSecurityGroupObjectId)
{
    $emergencyAccessUser = createEmergencyAccessAzureADUser $displayName $password
    assignAzureADUserToAzureADGroupAndMakeOwner $emergencyAccessAzureAdSecurityGroupObjectId $emergencyAccessUser.UserPrincipalName
    assignEmergencyAccessAzureADUserToAzureADGlobalAdministratorRole $emergencyAccessUser.UserPrincipalName    
}


Connect-AzureAD -TenantId $TenantId

# Create the security group to hold all break-glass users (for en-mass exclusion targeting)
$emergencyAccessAzureAdSecurityGroup = createEmergencyAccessAzureAdSecurityGroup

# Create two emergency access users
provisionEmergencyAccessAzureADUser "Everest" (ConvertTo-SecureString -String "<complex passworD5%>" -AsPlainText -Force) $emergencyAccessAzureAdSecurityGroup.ObjectId
provisionEmergencyAccessAzureADUser "Mariana" (ConvertTo-SecureString -String "<different complex passworD5%>" -AsPlainText -Force) $emergencyAccessAzureAdSecurityGroup.ObjectId

Disconnect-AzureAD