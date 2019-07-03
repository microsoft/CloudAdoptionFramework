Import-Module Az.Automation

$SubscriptionID = "16471a83-9151-456e-bbb1-463027bed604"
$RG = "RG6"
$AAccount = "AAJY06"
$WorkspaceName = "WSJY6"

$scheduleName = "SatPatches2"
$startTime = ([DateTime]::Now).AddMinutes(10)
$schedule = New-AzAutomationSchedule -ResourceGroupName $RG `
                                       -AutomationAccountName $AAccount  `
                                       -StartTime $startTime `
                                       -Name $scheduleName `
                                       -Description "Patch on Saturdays" `
                                       -DaysOfWeek Saturday `
                                       -WeekInterval 1 `
                                       -ForUpdateConfiguration

# Using AzAutomationUpdateManagementAzureQuery to create dynamic groups
$queryScope = @("/subscriptions/$SubscriptionID/resourceGroups/")

$query1Location =@("westus", "eastus", "eastus2")
$query1FilterOperator = "Any"
$ownerTag = @{"Owner"= @("JaneSmith")}
$ownerTag.add("Production", "true")

$DGQuery = New-AzAutomationUpdateManagementAzureQuery -ResourceGroupName $RG `
                                       -AutomationAccountName $AAccount `
                                       -Scope $queryScope `
                                       -Tag $ownerTag

$AzureQueries = @($DGQuery)

$nonAzureQuery1 = @{
    FunctionAlias = "ChangeTracking_MicrosoftDefaultComputerGroup";
#    WorkspaceResourceId = "/subscriptions/$SubscriptionID/resourcegroups/$RG/providers/microsoft.operationalinsights/workspaces/$WorkspaceName"
    WorkspaceResourceId = "/subscriptions/$SubscriptionID/resourcegroups/rg6/providers/microsoft.operationalinsights/workspaces/$WorkspaceName"
}

#$NonAzureQueries = @($nonAzureQuery1, $nonAzureQuery2)
$NonAzureQueries = @($nonAzureQuery1)

$UpdateConfig = New-AzAutomationSoftwareUpdateConfiguration  -ResourceGroupName $RG `
                                                             -AutomationAccountName $AAccount `
                                                             -Schedule $schedule `
                                                             -Windows `
                                                             -Duration (New-TimeSpan -Hours 2) `
                                                             -AzureQuery $AzureQueries `
                                                             -NonAzureQuery $NonAzureQueries `
                                                             -IncludedUpdateClassification Security,Critical
                                                         

#$UpdateConfig = New-AzAutomationSoftwareUpdateConfiguration  -ResourceGroupName $RG `
#                                                             -AutomationAccountName $AAccount `
#                                                             -Schedule $schedule `
#                                                             -Windows `
#                                                             -Duration (New-TimeSpan -Hours 2) `
#                                                             -AzureQuery $AzureQueries `
#                                                             -IncludedUpdateClassification Security,Critical