
**Learn more about Blueprints at The Azure Academy**
============================
	https://www.youtube.com/AzureAcademy
	
	
**Get Azure  Info**
============================
    $MgmtName = '<ENTER MANAGEMENT GROUP NAME>'
    $SubName = '<ENTER SUBSCRIPTION NAME>'
    $SubID = (Get-AzSubscription `
        -SubscriptionName $SubName).id
    $MgmtID = (Get-AzManagementGroup -GroupName $MgmtName).id.Split('/')[4]


**Get PowerShell Script to manage Azure Blueprints**
============================
	Install-Module -Name Az.Blueprint `
	    -Repository PSGallery `
	    -MinimumVersion 0.2.1 `
	    -AllowClobber `
	    -Force `
	    -Verbose
	Import-Module `
	    -Name Az.Blueprint


**Export Blueprint from Management Group**
============================
    $LocalPath = 'C:\temp\Blueprint'
    $BPs = Get-AzBlueprint -ManagementGroupId $MgmtID
    foreach ($BP in $BPs) {    
    Export-AzBlueprintWithArtifact `
        -Blueprint $BP `
        -OutputPath $LocalPath `
        -Force `
        -Verbose
    }


**Import Blueprint to Management Group**
============================
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
	
	
**Export Blueprint from Subscription**
============================
    $LocalPath = 'C:\temp\Blueprint'
    $BPs = Get-AzBlueprint -SubscriptionId $SubID
    foreach ($BP in $BPs) {    
    Export-AzBlueprintWithArtifact `
        -Blueprint $BP `
        -OutputPath $LocalPath `
        -Force `
        -Verbose
    }


**Import Blueprint to Subscription**
============================
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


**END**
============================
