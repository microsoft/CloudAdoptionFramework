  <#Author   : Dean Cefola
# Creation Date: 05-20-2019
# Usage      : AZURE Blueprint - Export and Import as Code 

#******************************************************************************
# Date                         Version      Changes
#------------------------------------------------------------------------------
# 05/20/2019                       1.0       Intial Version
# 06/25/2019                       2.0       Convert to Az.Blueprint modules
# 06/26/2019                       3.0       Add Management Group & Subscription Code
#
#******************************************************************************
#
#>


########################
#    Get Azure  Info   #
########################
$MgmtName = '<ENTER MANAGEMENT GROUP NAME>'
$SubName = '<ENTER SUBSCRIPTION NAME>'
$SubID = (Get-AzSubscription `
    -SubscriptionName $SubName).id
$MgmtID = (Get-AzManagementGroup -GroupName $MgmtName).id.Split('/')[4]


##########################################################
#    Get PowerShell Script to manage Azure Blueprints    #
##########################################################
Install-Module -Name Az.Blueprint `
    -Repository PSGallery `
    -MinimumVersion 0.2.1 `
    -AllowClobber `
    -Force `
    -Verbose
Import-Module `
    -Name Az.Blueprint


################################################
#    Export Blueprint from Management Group    #
################################################
$LocalPath = 'C:\temp\Blueprint'
$BPs = Get-AzBlueprint -ManagementGroupId $MgmtID
foreach ($BP in $BPs) {    
   Export-AzBlueprintWithArtifact `
            -Blueprint $BP `
            -OutputPath $LocalPath `
            -Force `
            -Verbose
}


##############################################
#    Import Blueprint to Management Group    #
##############################################
$LocalPath='C:\temp\Blueprint\'
Set-Location $LocalPath
$BPFolders = Get-ChildItem $LocalPath
foreach($BPFolder in $BPFolders) {
    $BPName = $BPFolder.Name
    Import-AzBlueprintWithArtifact `
        -Name $BPName `
        -InputPath $BPFolder.FullName `
        -ManagementGroupId $MgmtID `
        -Force `
        -Verbose
}


############################################
#    Export Blueprint from Subscription    #
############################################
$LocalPath = 'C:\temp\Blueprint'
$BPs = Get-AzBlueprint -SubscriptionId $SubID
foreach ($BP in $BPs) {    
   Export-AzBlueprintWithArtifact `
            -Blueprint $BP `
            -OutputPath $LocalPath `
            -Force `
            -Verbose
}


##########################################
#    Import Blueprint to Subscription    #
##########################################
$LocalPath='C:\temp\Blueprint\'
Set-Location $LocalPath
$BPFolders = Get-ChildItem $LocalPath
foreach($BPFolder in $BPFolders) {
    $BPName = $BPFolder.Name
    Import-AzBlueprintWithArtifact `
        -Name $BPName `
        -InputPath $BPFolder.FullName `
        -SubscriptionId $SubID `
        -Force `
        -Verbose
}


#
