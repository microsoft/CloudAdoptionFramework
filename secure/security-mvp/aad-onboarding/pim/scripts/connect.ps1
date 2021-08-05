#
# If you require multi-factor authentication for role activation, there is currently no way 
# for PowerShell to challenge the user when they activate their role. Instead, users will 
# need to trigger the MFA challenge when they connect to Azure AD by following 
# this blog post http://www.anujchaudhary.com/2020/02/connect-to-azure-ad-powershell-with-mfa.html
# from one of our engineers.
#
# Get token for MS Graph by prompting for MFA
#
#$MsResponse = Get-MSALToken -Scopes @("https://graph.microsoft.com/.default") -ClientId "1b730954-1685-4b74-9bfd-dac224a7b894" -RedirectUri "urn:ietf:wg:oauth:2.0:oob" -Authority "https://login.microsoftonline.com/common" -Interactive -ExtraQueryParameters @{claims='{"access_token" : {"amr": { "values": ["mfa"] }}}'}
#$AadResponse = Get-MSALToken -Scopes @("https://graph.windows.net/.default") -ClientId "1b730954-1685-4b74-9bfd-dac224a7b894" -RedirectUri "urn:ietf:wg:oauth:2.0:oob" -Authority "https://login.microsoftonline.com/common"
#Connect-AzureAD -AccountId $AccountId -TenantId $TenantId -AadAccessToken $AadResponse.AccessToken -MsAccessToken $MsResponse.AccessToken

# Connect to Azure AD
Connect-AzureAD -AccountId $AccountId -TenantId $TenantId
