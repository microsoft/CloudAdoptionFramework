#Create a service principal for the Key Vault in the Blueprint, 
#The objectId will be listed in the output as Id (not ApplicationId).
New-AzADServicePrincipal -DisplayName CAFKeyVaultsp

#Get service principal
Get-AzADServicePrincipal -DisplayName CAFKeyVaultsp