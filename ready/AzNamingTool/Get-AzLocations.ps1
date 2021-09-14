$AzureCloud = 'AzureCloud'
$AzLocationsCSV = ".\AzLocations.csv"

$Endpoint = switch($AzureCloud)
{
    AzureChinaCloud {'management.chinacloudapi.cn'}
    AzureCloud {'management.azure.com'}
    AzureUSGovernment {'management.usgovcloudapi.net'}
}

# Log in first with Connect-AzAccount
$azContext = Get-AzContext
$azProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
$profileClient = New-Object -TypeName Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient -ArgumentList ($azProfile)
$token = $profileClient.AcquireAccessToken($azContext.Subscription.TenantId)
$Header = @{
    'Content-Type'='application/json'
    'Authorization'='Bearer ' + $token.AccessToken
}
$subscriptionId = $azContext.Subscription.Id
$URI = "https://$Endpoint/subscriptions/$subscriptionId/locations?api-version=2020-01-01"
$Locations = (Invoke-RestMethod `
    -Headers $Header `
    -Method Get `
    -Uri $URI).value


$Locations = $Locations | Sort-Object -Property {$_.metadata.geographyGroup}

# CSV Header
# Name  |  Short  | geographyGroup | physicalLocation | pairedRegion (pairedRegion.name) | Lat | Long | Type | regionCategory 
$CSVHeader = @"
Name,Short,geographyGroup,physicalLocation,pairedRegion,Lat,Long,Type,regionCategory
"@
$CSVHeader | Out-File $AzLocationsCSV -Encoding ascii

# Get codes for each and build array with all data

# Maintain an array of location codes to check for any duplicates
$Codes = @()
$csvLine = @()



# Iterate over the $locations object to generate location codes
Foreach($Location in $Locations){
    # Filter out staging locations
    If(($location.displayName -notmatch '(Stage)') -and ($Location.displayName -notmatch 'EUAP') -and ($Location.metadata.physicalLocation -ne $null)){
        # location Code generation logic based on first character of each word in Display name
        $names = ($Location.displayName -split " ")
        Foreach($name in $names){
            If($name -notmatch 'EUAP'){$value += $name[0]}  #omit (Stage) and EUAP portion of name
        }

        If($value.length -lt 2){$value = ($Location.displayName).Substring(0,2)}
    
        # Check for duplicate codes in array and Increment in case of duplicate
        if($Codes -eq $value){$Value = $value + "2"}

        # Add code generated to $codes array
        $Codes += $Value
    
        $csvLine = $Location.displayname + ","
        $csvLine += $value.ToLower() + ","
        $csvLine += $Location.metadata.geographyGroup + ","
        If($Location.metadata.physicalLocation -match ','){$temp += $Location.metadata.physicalLocation -replace ',', ' '
            $csvLine += $temp + "," }
        else{$csvLine += $Location.metadata.physicalLocation + ","}
        $csvLine += $Location.metadata.pairedregion.name + ","
        $csvLine += $Location.metadata.latitude + ","
        $csvLine += $Location.metadata.longitude + ","
        $csvLine += $Location.metadata.regionType + ","
        $csvLine += $Location.metadata.regionCategory
        
        $temp = $null   
        $value = $null
        $csvLine | Out-File $AzLocationsCSV -Append ascii

    } # End If not staging
}
#$codes = $null