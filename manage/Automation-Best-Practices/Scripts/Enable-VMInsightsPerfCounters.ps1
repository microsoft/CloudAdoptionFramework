<#PSScriptInfo

.VERSION 1.01

.GUID 06461afa-14a1-4e68-87dd-9e42203ac0be

.AUTHOR dougbrad@microsoft.com

.COMPANYNAME

.COPYRIGHT

.TAGS

.LICENSEURI

.PROJECTURI

.ICONURI

.EXTERNALMODULEDEPENDENCIES

.REQUIREDSCRIPTS

.EXTERNALSCRIPTDEPENDENCIES

.RELEASENOTES

.PRIVATEDATA

#>

<#
.SYNOPSIS
  Enables collection of Performance Counters used by VM Insights to a Log Analytics Workspace

.Description
  This script takes parameters: WorkspaceName and WorkspaceResourceGroupName
  The Log Analytics Workspace is configured to collect counters used by VM Insights, see the variable $countersToAddJson for counters that are added.

  For more info on Log Analytics Performance Counters, see:
  https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-data-sources-performance-counters

.PARAMETER WorkspaceName
    Name of Log Analytics Workspace to configure

.PARAMETER WorkspaceResourceGroupName
    Resource Group the Log Analytics Workspace is in

.EXAMPLE
  .\Enable-VMInsightsPerfCounters.ps1 -WorkspaceName <Name of Workspace> -WorkspaceResourceGroupName <Workspace Resource Group>

.LINK
    This script is posted to and further documented at the following location:
    http://aka.ms/OnBoardVMInsights
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(mandatory = $true)][string]$WorkspaceName,
    [Parameter(mandatory = $true)][string]$WorkspaceResourceGroupName
)
#
# FUNCTIONS
#

function Get-CounterInConfiguredCounters {
    <#
	.SYNOPSIS
    Return the Counter if found in list of ExistingCounters
    For LinuxPerformanceObject if object and instance match as this is identifier for a definition
	#>
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)][psobject]$NewCounter,
        [Parameter(mandatory = $true)][psobject]$ExistingCounters
    )

    if ($NewCounter.Kind -eq "WindowsPerformanceCounter") {
        foreach ($existingCounter in $ExistingCounters) {
            if ($existingCounter.Properties.objectName -eq $NewCounter.properties.objectName -and
                $existingCounter.Properties.counterName -eq $NewCounter.properties.counterName -and
                $existingCounter.Properties.instanceName -eq $NewCounter.properties.instanceName) {
                Write-Verbose("Counter: " + $NewCounter.name + " with same objectName, counterName, instanceName found")
                return $existingCounter
            }
        }
        Write-Verbose("Counter with settings of: " + $NewCounter.Name + " not found")
    }
    elseif ($NewCounter.kind -eq "LinuxPerformanceObject") {
        foreach ($existingCounter in $ExistingCounters) {
            if ($existingCounter.Properties.objectName -eq $NewCounter.properties.objectName -and
                $existingCounter.Properties.instanceName -eq $NewCounter.properties.instanceName) {
                Write-Verbose("Counter: " + $NewCounter.name + " with same objectName, instanceName found")
                return $existingCounter
            }
        }
        Write-Verbose("Counter with settings of: " + $NewCounter.Name + " not found")
    }
}

# List of Counters to configure for the workspace
# Can deploy through an ARM template but the values for 'name' must be updated based on configuration state of the workspace
$countersToAddJson = @"
[
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Pct-Free-Space",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "% Free Space"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Avg-DiskSecRead",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Avg. Disk sec/Read"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Avg-DiskSecTransfer",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Avg. Disk sec/Transfer"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Avg-DiskSecWrite",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Avg. Disk sec/Write"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Disk-BytesSec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Disk Bytes/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Disk-ReadBytesSec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Disk Read Bytes/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Disk-ReadsSec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Disk Reads/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Disk-TransfersSec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Disk Transfers/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Disk-WriteBytesSec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Disk Write Bytes/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-Disk-WritesSec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Disk Writes/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-LogicalDisk-FreeMegabytes",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "LogicalDisk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Free Megabytes"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-Memory-AvailableMBytes",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "Memory",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Available MBytes"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-NetworkAdapter-BytesReceived-sec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "Network Adapter",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Bytes Received/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-NetworkAdapter-BytesSent-sec",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "Network Adapter",
            "instanceName": "*",
            "intervalSeconds": 60,
            "counterName": "Bytes Sent/sec"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-Processor-Pct-Processor-Time-Total",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "WindowsPerformanceCounter",
        "properties": {
            "objectName": "Processor",
            "instanceName": "_Total",
            "intervalSeconds": 60,
            "counterName": "% Processor Time"
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-Logical-Disk-Linux",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "LinuxPerformanceObject",
        "properties": {
            "objectName": "Logical Disk",
            "instanceName": "*",
            "intervalSeconds": 60,
            "performanceCounters": [
                {
                    "counterName": "% Used Space"
                },
                {
                    "counterName": "Disk Read Bytes/sec"
                },
                {
                    "counterName": "Disk Reads/sec"
                },
                {
                    "counterName": "Disk Transfers/sec"
                },
                {
                    "counterName": "Disk Write Bytes/sec"
                },
                {
                    "counterName": "Disk Writes/sec"
                },
                {
                    "counterName": "Free Megabytes"
                },
                {
                    "counterName": "Logical Disk Bytes/sec"
                }
            ]
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-Memory-Linux",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "LinuxPerformanceObject",
        "properties": {
            "objectName": "Memory",
            "instanceName": "*",
            "intervalSeconds": 60,
            "performanceCounters": [
                {
                    "counterName": "Available MBytes Memory"
                }
            ]
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-Network",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "LinuxPerformanceObject",
        "properties": {
            "objectName": "Network",
            "instanceName": "*",
            "intervalSeconds": 60,
            "performanceCounters": [
                {
                    "counterName": "Total Bytes Received"
                },
                {
                    "counterName": "Total Bytes Transmitted"
                }
            ]
        }
    },
    {
        "apiVersion": "2015-11-01-preview",
        "type": "datasources",
        "name": "VMInsights-Processor-Pct-Processor-Time-Linux",
        "dependsOn": [
            "[parameters('WorkspaceResourceId')]"
        ],
        "kind": "LinuxPerformanceObject",
        "properties": {
            "objectName": "Processor",
            "instanceName": "*",
            "intervalSeconds": 60,
            "performanceCounters": [
                {
                    "counterName": "% Processor Time"
                }
            ]
        }
    }
]
"@

$countersToAdd = $countersToAddJson | ConvertFrom-Json
$DefaultIntervalSeconds = 60

#
# Main Script
#
Enable-AzureRmAlias

# Enable Linux Performance Collection (no way from PowerShell to check if already enabled)
Write-Output("Enabling Linux Performance Collection")
Enable-AzureRmOperationalInsightsLinuxPerformanceCollection -WorkspaceName $WorkspaceName -ResourceGroupName $WorkspaceResourceGroupName

Write-Output("Getting existing Windows Performance Counter Configuration")
$existingWindowsPerfCounters = Get-AzureRmOperationalInsightsDataSource -WorkspaceName $WorkspaceName -ResourceGroupName $WorkspaceResourceGroupName -Kind WindowsPerformanceCounter

Write-Output("Getting existing Linux Performance Counter Configuration")
$existingLinuxPerfCounters = Get-AzureRmOperationalInsightsDataSource -WorkspaceName $WorkspaceName -ResourceGroupName $WorkspaceResourceGroupName -Kind LinuxPerformanceObject

foreach ($newCounter in $countersToAdd ) {
    #
    # Windows
    #
    if ($newCounter.kind -eq "WindowsPerformanceCounter") {
        $existingCounterDataSource = $null
        if ($existingWindowsPerfCounters) {
            $existingCounterDataSource = Get-CounterInConfiguredCounters -NewCounter $newCounter -ExistingCounters $existingWindowsPerfCounters
        }
        if ($existingCounterDataSource) {
            Write-Output("Collection already configured:`n  Name: " + $existingCounterDataSource.Name + " Object: " `
                    + $existingCounterDataSource.properties.objectName + " Instance: " `
                    + $existingCounterDataSource.properties.instanceName + " Counter: " `
                    + $existingCounterDataSource.properties.counterName)
        }
        else {
            Write-Output("Configuring collection with Name: " + $newCounter.name + " ObjectName: " + $newCounter.properties.objectName + `
                    " CounterName: " + $newCounter.properties.counterName + " InstanceName: " + $newCounter.properties.instanceName)
            New-AzureRmOperationalInsightsWindowsPerformanceCounterDataSource `
                -WorkspaceName $WorkspaceName `
                -ResourceGroupName $WorkspaceResourceGroupName `
                -ObjectName $newCounter.properties.objectName `
                -CounterName $newCounter.properties.counterName `
                -InstanceName $newCounter.properties.instanceName `
                -Name $newCounter.name `
                -IntervalSeconds $DefaultIntervalSeconds
        }
    }
    #
    # Linux
    #
    elseif ($newCounter.kind -eq "LinuxPerformanceObject") {
        $existingCounterDataSource = $null
        if ($existingLinuxPerfCounters) {
            $existingCounterDataSource = Get-CounterInConfiguredCounters -NewCounter $newCounter -ExistingCounters $existingLinuxPerfCounters
        }

        if ($existingCounterDataSource) {
            Write-Verbose("Counter: " + $NewCounter.name + " with same objectName, instanceName found. Need to check if need to update with additinal counters.")

            # Put existing configured counters into a Powershell Array
            $existingCounterNamesArray = @()
            foreach ($existingCounter in $existingCounterDataSource.properties.performanceCounters) {
                $existingCounterNamesArray += $existingCounter.counterName
            }

            # Put our set counters into a PowerShell Array
            $newCounterNamesArray = @()
            foreach ($counter in $newCounter.properties.performanceCounters) {
                $newCounterNamesArray += $counter.counterName
            }

            # Combine and remove duplicates
            $allCounters = $newCounterNamesArray + $newCounterNamesArray
            $allCounters = $allCounters | Select-Object -uniq

            if ($allCounters.Count -gt $existingCounterNamesArray.Count) {
                Write-Output("Updating collection with Name: " + $newCounter.name + `
                        " ObjectName: " + $existingCounterDataSource.Properties.objectName + `
                        " CounterNames: " + $counterNamesArray + " InstanceName: " + $newCounter.properties.instanceName)

                # Update with new counters, using the existing settings for Name, IntervalSeconds, ...
                New-AzureRmOperationalInsightsLinuxPerformanceObjectDataSource `
                    -WorkspaceName $WorkspaceName `
                    -ResourceGroupName $WorkspaceResourceGroupName `
                    -ObjectName $existingCounterDataSource.Properties.objectName `
                    -CounterNames $allCounters  `
                    -InstanceName $existingCounterDataSource.properties.instanceName `
                    -Name $existingCounterDataSource.name `
                    -IntervalSeconds $existingCounterDataSource.properties.intervalSeconds `
                    -Force
            }
            else {
                Write-Output("Collection already configured:`n  Name: " + $existingCounterDataSource.Name + " Object: " `
                        + $existingCounterDataSource.properties.objectName + " Instance: " `
                        + $existingCounterDataSource.properties.instanceName + " Counters: " `
                        + $existingCounterNamesArray )
            }

        }

        # No existing Perf DataSource with same object and instance
        else {
            # Put counters into a PowerShell Array
            $counterNamesArray = @()
            foreach ($counter in $newCounter.properties.performanceCounters) {
                $counterNamesArray += $counter.counterName
            }

            Write-Output("Creating a counter with Name: " + $newCounter.name + " ObjectName: " + $newCounter.properties.objectName + `
                    " CounterName: " + $counterNamesArray + " InstanceName: " + $newCounter.properties.instanceName)

            New-AzureRmOperationalInsightsLinuxPerformanceObjectDataSource `
                -WorkspaceName $WorkspaceName `
                -ResourceGroupName $WorkspaceResourceGroupName `
                -ObjectName $newCounter.properties.objectName `
                -CounterNames $counterNamesArray `
                -InstanceName $newCounter.properties.instanceName `
                -Name $newCounter.name `
                -IntervalSeconds $DefaultIntervalSeconds
        }
    }
}