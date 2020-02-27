#https://blog.ipswitch.com/managing-azure-blueprints-with-powershell

#Connect to Azure from local
Connect-AzAccount -UseDeviceAuthentication

#Use Azure Cloudshell for Mac users - remember $Home\clouddrive

#Install Azure Blueprint module
Install-Module -Name Az.Blueprint -Verbose -Scope CurrentUser; Update-Help -Force -ErrorAction SilentlyContinue

#Get the Blueprint you would like to export, this sample has this as a subscrition level Blueprint
$blueprint = Get-AzBlueprint -Name '<Blueprint name>' -SubscriptionId '<Subscription ID>' -Version '<version>'

#Export Blueprint and make changes with VS Code
Export-AzBlueprintWithArtifact -Blueprint $blueprint -OutputPath '.\blueprints'

#Import Blueprint as draft to review first then save and assign 
Import-AzBlueprintWithArtifact -Name '<Blueprint name>' -SubscriptionId '<Subscription ID>' -InputPath  '.\blueprints\<Blueprint name>'

