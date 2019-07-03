#Assign Guest configuration policy
param (
	[Parameter(Mandatory=$true)]
	[string]$SubscriptionName
)

$Subscription = Get-AzSubscription -SubscriptionName $SubscriptionName
$scope = "/subscriptions/" + $Subscription.Id

$PasswordPolicy = Get-AzPolicySetDefinition -Name "3fa7cbf5-c0a4-4a59-85a5-cca4d996d5a6"
$CertExpirePolicy = Get-AzPolicySetDefinition -Name "b6f5e05c-0aaa-4337-8dd4-357c399d12ae"

New-AzPolicyAssignment -Name "PasswordPolicy" -DisplayName "[Preview]: Audit that password security settings are set correctly inside Linux and Windows machines" -Scope $scope -PolicySetDefinition $PasswordPolicy ` -AssignIdentity -Location eastus

New-AzPolicyAssignment -Name "CertExpirePolicy" -DisplayName "[Preview]: Audit that certificates are not expiring on Windows VMs" -Scope $scope -PolicySetDefinition $CertExpirePolicy ` 
-AssignIdentity -Location eastus
