$Components = @(
    'ResourceType',
    'Org',
    'UnitDept',
    'ProjAppSvc',
    'VmRole',
    'Environment',
    'Location',
    'Instance'
)
$Delimiter = '-'
$Org = [pscustomobject][ordered]@{Name = "FastTrack for Azure"; Short = "fta"}
$Locations = Import-Csv -Path '.\configs\locations.csv' -ErrorAction Stop
$Environments = Import-Csv -Path '.\configs\environments.csv' -ErrorAction Stop
$Resources = Import-Csv '.\configs\resources.csv' -ErrorAction Stop | Sort-Object resource
$ProjAppSvc = @(
    [pscustomobject][ordered]@{Name = "Auris"; Short = "aur"},
    [pscustomobject][ordered]@{Name = "Ceres"; Short = "cer"},
    [pscustomobject][ordered]@{Name = "Oslo"; Short = "osl"}
)
$UnitDept = @(
    [pscustomobject][ordered]@{Name = "Americas"; Short = "am"},
    [pscustomobject][ordered]@{Name = "Americas Federal"; Short = "amf"},
    [pscustomobject][ordered]@{Name = "Asia"; Short = "as"},
    [pscustomobject][ordered]@{Name = "Emea"; Short = "em"}
)
$VmRoles = Import-Csv '.\configs\vmRoles.csv' -ErrorAction Stop | Sort-Object Name
$Location = Import-Csv -Path '.\configs\locations.csv' -ErrorAction Stop
$Object = New-Object PSObject
foreach($Component in $Components)
{
    if($Component -eq 'Environment'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Environments
    }
    elseif($Component -eq 'Instance'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value "001"
    }
    elseif($Component -eq 'Location'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $($Location | Sort-Object Name | Select-Object -Property Name,Short)
    }
    elseif($Component -eq 'Org'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Org
    }
    elseif($Component -eq 'ProjAppSvc'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $ProjAppSvc
    }
    elseif($Component -eq 'ResourceType'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Resources
    }
    elseif($Component -eq 'UnitDept'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $UnitDept
    }
    elseif($Component -eq 'VmRole'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $VmRoles
    }
}

$Policy = [pscustomobject][ordered]@{
    if = ''
    then = ''
}

$PolicyJson = $([System.Text.RegularExpressions.Regex]::Unescape($($Policy | ConvertTo-Json -Depth 100)))

#
#  Figure out the substring postion for each component and create conditions for each option
#  
#

$String = 'as-amf-eu2-001'
$String = 'asamfeu2001'

$String = 'stor-amf-eu2-001'

$Array = @('am', 'amf', 'as', 'em')
$Substring = $String.Substring(3,3)

$Org.short.Length

$Lengths = foreach($obj in $Object.UnitDept)
{
    $obj.short.length
}

$Lengths | Select-Object -Unique