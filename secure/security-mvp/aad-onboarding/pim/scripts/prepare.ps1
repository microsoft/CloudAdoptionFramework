# Install AzureADPreview
if(!(Get-Package AzureADPreview)) { 
    Install-module AzureADPreview -AllowClobber -Force
}

# Install MSAL.PS
if(!(Get-Package MSAL.PS)) { 
   Install-PackageProvider NuGet -Force
   Install-Module PowerShellGet -Force
   &(Get-Process -Id $pid).Path -Command { Install-Module MSAL.PS -Force -AcceptLicense }
   Import-Module MSAL.PS
}
