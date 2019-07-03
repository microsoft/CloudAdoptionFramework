<#
    .SYNOPSIS
        This script rolls orchestrate the deployment of the solutions and the agents.
    .Parameter SubscriptionName
    .Parameter WorkspaceName
    .Parameter AutomationAccountName
    .Parameter WorkspaceLocation
    .Parameter AutomationAccountLocation

    .Example
    .\New-AMSDeployment.ps1 -SubscriptionName 'Subscription Name' -WorkspaceName 'WorkspaceName' -WorkspaceLocation 'eastus' -AutomationAccountName -AutomationAccountLocation

    .Notes
    PolicySet '[Preview]: Enable Azure Monitor for VMs' is assigned with name VMInsightPolicy. 
#>

param (
    [Parameter(Mandatory=$true, HelpMessage="Subscription Name" ) ]
    [string]$SubscriptionName,

    [Parameter(Mandatory=$true, HelpMessage="Resource Group Name" )]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$false, HelpMessage="Resource Group location. Defaults to EastUS" )]
    [string]$ResourceGroupLocation = "eastus",

    [Parameter(Mandatory=$true, HelpMessage="Workspace Name" )]
    [string]$WorkspaceName,

    [Parameter(Mandatory=$false, HelpMessage="Workspace location. Defaults to EastUS" )]
    [string]$WorkspaceLocation = "eastus",

    [Parameter(Mandatory=$true, HelpMessage="Automation Account Name" )]
    [string]$AutomationAccountName,

    [Parameter(Mandatory=$false, HelpMessage="Automation Account location. Defaults to EastUS2" )]
    [string]$AutomationAccountLocation = "eastus2",

    [Parameter(Mandatory=$false, HelpMessage="Auto Enroll." )]
    [string]$AutoEnroll = $false

)

# Script settings
Set-StrictMode -Version Latest

function ThrowTerminatingError
{
     Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $ErrorId,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $ErrorCategory,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        $Exception
    )

    $errorRecord = [System.Management.Automation.ErrorRecord]::New($Exception, $ErrorId, $ErrorCategory, $null)
    throw $errorRecord
}

#
# Automation account requires 6 chars at minimum
#
if ( $AutomationAccountName.Length -lt 6 )
{
    $message = "Automation account name validation failed: The name can contain only letters, numbers, and hyphens. The name must start with a letter, and it must end with a letter or a number. The account name length must be from 6 to 50 characters"
    ThrowTerminatingError -ErrorId "InvalidAutomationAccountName" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message `
}

#
# Check all dependency files exist along with this script
#
$curInvocation = get-variable myinvocation
$mydir = split-path $curInvocation.Value.MyCommand.path

$enableVMInsightsPerfCounterScriptFile = "$mydir\Enable-VMInsightsPerfCounters.ps1"
if (-not (Test-Path -Path $enableVMInsightsPerfCounterScriptFile))
{
    $message = "$enableVMInsightsPerfCounterScriptFile does not exist. Please ensure this file exists in the same directory as the this script."
    ThrowTerminatingError -ErrorId "ScriptNotFound" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message `
}

$changeTrackingTemplateFile = "$mydir\..\Templates\ChangeTracking-Filelist.json"
if (-not (Test-Path -Path $changeTrackingTemplateFile ) )
{
    $message = "$changeTrackingTemplateFile does not exist. Please ensure this file exists in the same directory as the this script."
    ThrowTerminatingError -ErrorId "TemplateNotFound" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message `
}

$scopeConfigTemplateFile = "$mydir\..\Templates\ScopeConfig.json"
if (-not (Test-Path -Path $scopeConfigTemplateFile ) )
{
    $message = "$scopeConfigTemplateFile does not exist. Please ensure this file exists in the same directory as the this script."
    ThrowTerminatingError -ErrorId "TemplateNotFound" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message `
}

$workspaceAutomationTemplateFile = "$mydir\..\Templates\Workspace-AutomationAccount.json"
if (-not (Test-Path -Path $workspaceAutomationTemplateFile ) )
{
    $message = "$workspaceAutomationTemplateFile does not exist. Please ensure this file exists in the same directory as the this script."
    ThrowTerminatingError -ErrorId "TemplateNotFound" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message `
}

$workspaceSolutionsTemplateFile = "$mydir\..\Templates\WorkspaceSolutions.json"
if (-not (Test-Path -Path $workspaceSolutionsTemplateFile ) )
{
    $message = "$workspaceSolutionsTemplateFile does not exist. Please ensure this file exists in the same directory as the this script."
    ThrowTerminatingError -ErrorId "TemplateNotFound" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message `
}

#
# Choose the right subscription
#
try
{
    $subscription = Get-AzSubscription -SubscriptionName $SubscriptionName  -ErrorAction Stop
}
catch 
{
    ThrowTerminatingError -ErrorId "FailedToGetSubscriptionInformation" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidResult) `
        -Exception $_.Exception `
}

#
# Check if user is owner of the subscription
#
$azContext = Get-AzContext
$currentUser = $azContext.Account.Id
$userRole = Get-AzRoleAssignment -SignInName $currentUser -RoleDefinitionName Owner -Scope "/subscriptions/$($Subscription.Id)"
if (-not $userRole)
{
    $message = "Insufficient permissions for Policy assignment."
    ThrowTerminatingError -ErrorId "UserUnAuthorizedForPolicyAssignment" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message
}

#
# Create the Resource group if not exist
#
$newResourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -ErrorAction SilentlyContinue
If (-not $NewResourceGroup)
{
    Write-Output "Creating resource group: $($ResourceGroupName)"
    $newResourceGroup = New-AzResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation
}
else
{
    #
    # We are intentionally not trying to re-use an existing resource group. 
    # For e.g. if it exists, but not in the location that was requested, could set us in an inconsistent state.
    #
    $message = "ResourceGroup: $($ResourceGroupName) already exists. Please use a new resource group."
    ThrowTerminatingError -ErrorId "ResourceGroupAlreadyExists" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message
}

#
# Deploy Workspace and solutions
#
try 
{
    Write-Output "Phase 1 Deployment start: Create resources"

    #
    # Check for workspace and automation account name to be unique in the subscription across resoure groups
    # Duplicate names can cause "bad request" error.
    #

    #
    # Check if the automation account already exists
    #
    $existingAutomationAccount = Get-AzResource -Name $AutomationAccountName -ResourceType "Microsoft.Automation/automationAccounts" -ErrorAction SilentlyContinue
    if ($existingAutomationAccount)
    {
        $message = "Automation account: $AutomationAccountName already exists in this subscription. Please use a unique name."
        ThrowTerminatingError -ErrorId "AutomationAccountAlreadyExists" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
    }

    #
    # Check if the workspace already exists
    #
    $workspace = Get-AzResource -Name $WorkspaceName -ResourceType "Microsoft.OperationalInsights/workspaces" -ErrorAction SilentlyContinue
    if ($workspace)
    {
        $message = "Workspace: $WorkspaceName already exists. Please use a unique name."
        ThrowTerminatingError -ErrorId "WorkspaceAlreadyExists" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
    }

    #
    # Start deployment, provisioning Automation Account and Workspace
    #
    try
    {
        New-AzResourceGroupDeployment -Name "WorkSpaceAndAutomationAccountProvisioning" -ResourceGroupName $ResourceGroupName -TemplateFile $workspaceAutomationTemplateFile -workspaceName $WorkspaceName -workspaceLocation $WorkspaceLocation -automationName $AutomationAccountName -automationLocation $AutomationAccountLocation -ErrorAction Stop
    }
    catch
    {
        $message = "Automation Account and Workspace, provisioning failed. Details below... `n $_"
        ThrowTerminatingError -ErrorId "AutomationAccountAndWorkspaceProvisioningFailed" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
    }

    #
    # If we are here Automoation account and Workspace have been provisioned.
    #

    #
    # Enable solutions on the workspace
    #
    Write-Output "Phase 1 Deployment start: Enable solutions on the workspace"		
    try
    {
        New-AzResourceGroupDeployment -Name "EnableSolutions" -ResourceGroupName $ResourceGroupName -TemplateFile $workspaceSolutionsTemplateFile -workspaceName $WorkspaceName -WorkspaceLocation $WorkspaceLocation -Mode Incremental -ErrorAction Stop
    }
    catch
    {
        $message = "Solution provisioning on workspace failed. Detailed below... `n $_"
        ThrowTerminatingError -ErrorId "SolutionProvisioningFailed" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
    }
    
    If ($AutoEnroll -eq $false)
    {
        #
        # Check if default change tracking saved search group already exists
        # If not, add the default change tracking saved search and scope config
        #
        $savedSearch = Get-AzOperationalInsightsSavedSearch -ResourceGroupName $ResourceGroupName -WorkspaceName $WorkspaceName

        $createSavedSearch = $true
        foreach ($s in $savedSearch.Value)
        {
            if ($s.Id.Contains("changetracking|microsoftdefaultcomputergroup")) 
            {
                Write-output "Default saved search group already exists: $($s.Id)"
                $createSavedSearch = $false
                break
            }
        }

        if ($createSavedSearch)
        {
            Write-Output "Phase 1 Deployment start: Add scope config to the workspace"		

            try
            {
                New-AzResourceGroupDeployment -Name "AddScopeConfig" -ResourceGroupName $ResourceGroupName -TemplateFile $scopeConfigTemplateFile -workspaceName $WorkspaceName -WorkspaceLocation $Workspacelocation -Mode Incremental -ErrorAction Stop
            }
            catch
            {
                $message = "ScopeConfig provisioning on ResouceGroup failed. Detailed below... `n $_"
                ThrowTerminatingError -ErrorId "ScopeConfigProvisioningFailed" `
                -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
                -Exception $message 
            }
        }
    }

    #
    # Adding VMInsight configuration
    #
    Write-Output "Phase 1 Deployment start: Add VMInsight configuration"		

    #
    # Enabling perf counters on the workspace
    #
    try 
    {
        & $enableVMInsightsPerfCounterScriptFile -workspaceName $WorkspaceName -WorkspaceResourceGroupName $ResourceGroupName
    }
    catch
    {
        $message = "Failed to enble perf counter on the workspace: $WorkspaceName. Detailed below... `n $_ "
        ThrowTerminatingError -ErrorId "PerfCounterEnableFailed" `
        -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
        -Exception $message
    }

    #
    # Check if the workspace already exists
    #
    $workspace = Get-AzResource -Name $WorkspaceName -ResourceType "Microsoft.OperationalInsights/workspaces" -ErrorAction SilentlyContinue
    if (-not $workspace)
    {
        $message = "Workspace: $($WorkspaceName) does not exists. Please check."
        ThrowTerminatingError -ErrorId "WorkspaceNotFound" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
    }

    #
    # Assigning policies
    #
    $workspaceID = $workspace.ResourceId

    #
    # Get the VMInsight initiatives
    #
    $vmInsightPolicy = Get-AzPolicySetDefinition -Name "55f3eceb-5573-4f18-9695-226972c6d74a"

    Write-Output "Phase 1: assigning policies to workspace: $($workspaceID) to resourceGroup  $($ResourceGroupName) "

    #
    # Deploy the policy to the subscription
    #
    $subscriptionId = $subscription.Id
    $scope = "/subscriptions/$subscriptionId"
    Write-Output "Phase 1: Create policy VMInsightPolicy in scope : $scope"
    $policyparam = '{"logAnalytics_1": {"value":"' + $workspaceID +'"}}'	
    $newServicePrincipal = New-AzPolicyAssignment -Name "VMInsightPolicy" -DisplayName "Deploy VM Insight policy initiative" -Scope $scope -PolicySetDefinition $vmInsightPolicy -Location $AutomationAccountLocation -PolicyParameter $policyparam -AssignIdentity

    #
    # Check if the role assignment is already done for this service principal
    #

    $preAssignedRole = Get-AzRoleAssignment -ObjectId $newServicePrincipal.Identity.principalId -RoleDefinitionId 92aaf0da-9dab-42b6-94a3-d43ce8d16293 -Scope $scope
    
    #
    # If not, assign role for this service principal
    #
    if(-not $preAssignedRole)
    { 
        $retrytimes = 0
        Write-Output "Phase 1: assigning permission to the system identity."
        while ($retrytimes -le 10 )
        {
            try
            {
                $newRoleAssignment = New-AzRoleAssignment -ObjectId $newServicePrincipal.Identity.principalId -RoleDefinitionId 92aaf0da-9dab-42b6-94a3-d43ce8d16293 -scope $scope -ErrorAction Stop
                if($newRoleAssignment)
                {
                    break
                }
            }
            catch
            {
                $retrytimes ++
                Write-Output "Phase 1: waiting for the system identity to be created... trying $($retrytimes) times"
                Start-Sleep -s 5
            }
        }

        if (-not $newRoleAssignment)
        {
            #
            # The role didn't get created, the MMA extension will not work.
            #
            $message = "Phase1: Unable to assign the Log Analytics Contributor role to the managed identity, policy will not be able to deploy monitoring agent to the VMs. "
            ThrowTerminatingError -ErrorId "RoleAssignmentFailed" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
        }
    }
    else
    {
        Write-Output "Phase 1: Role assignment already done for VMInsightPolicy SPN"
    }

    #
    # Phase 2 deployment
    #
    Write-Output "Phase 2 Deployment start: Configure Change tracking"		

    try
    {
        New-AzResourceGroupDeployment -Name "ConfigureChangeTracking" -ResourceGroupName $ResourceGroupName -TemplateFile $changeTrackingTemplateFile -workspaceName $WorkspaceName -WorkspaceLocation $Workspacelocation -Mode Incremental -ErrorAction Stop
    }
    catch
    {
        $message = "ChangeTracking provisioning on ResouceGroup failed. Detailed below... `n $_"
        ThrowTerminatingError -ErrorId "ChangeTrackingProvisioningFailed" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
    }
}
catch 
{
    $message = "Deployment failed.`n $_"
        ThrowTerminatingError -ErrorId "DeploymentFailed" `
            -ErrorCategory ([System.Management.Automation.ErrorCategory]::InvalidOperation) `
            -Exception $message
}
