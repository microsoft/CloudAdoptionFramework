<#
.SYNOPSIS

Creates an Azure Naming Convention and stores your configuration in HTML file.

.DESCRIPTION

The Get-AzureNamingConvention.ps1 script creates an Azure Naming Convention.  This script will first 
login to Azure and collect all the supported locations for that Azure Cloud. Then it will ask you 
a series of questions to build out your naming convention.  Afterwards, your configuration will be 
exported out to HTML file.

.INPUTS

None. You cannot pipe objects to Export-AzurePolicyInitiative.ps1.

.OUTPUTS

The OUTPUT from this script will be an HTML file that contains your Naming Convention configuration that can be
used as a reference and generate names.

.EXAMPLE

PS> .\Set-AzureNamingConfiguration.ps1

#>

#################################################################
# Validate present working directory
#################################################################
try
{
    Get-ChildItem -Name Set-AzureNamingConfiguration.ps1 -ErrorAction Stop
}
catch
{
    throw 'ERROR: Your location / present working directory is incorrect.  Change your directory to the correct location.'
}


#################################################################
# Functions
#################################################################
function Confirm-Input
{
    do
    {
        Write-Host "Is this correct? Type Y for Yes or N for No"
        Write-Host ""
        $YesOrNo = Read-Host "Response"
        Write-Host ""
    }
    until ($YesOrNo -eq 'Y' -or $YesOrNo -eq 'N')
    return $YesOrNo
}

function Get-Description
{
    param($Component)
    switch($Component)
    {
        Org {'Organization name. Example: Contoso'}
        UnitDept {'Business Unit or Department. Example: Contoso US, Marketing, Marketing-Sales'}
        ProjAppSvc {'Project, Application, or Service Name. Example: Project Contoso, Application Contoso, SharePoint'}
        ResourceType {'Azure Resource Type (high level). Example: VM, Storage Account, Database, etc.'}
        VmRole {'The server role installed on the virtual machine. Example: Database, Domain Controller, etc.'}
        Environment {'Deployment Environment Type. Example: Dev, Test, Prod, Sandbox, Staging, Shared, UAT.'}
        Location {'Azure Data Center Location. Example: Azure West US, North Europe, Southeast Asia.'}
        Instance {'The instance of the deployment. Example: "001".'}
    }
}

function Get-Header
{
    Clear-Host
    Write-Host "######################################################################################################"
    Write-Host "#                                                                                                    #"
    Write-Host "# AZURE NAMING TOOL                                                                                  #"
    Write-Host "#                                                                                                    #"
    Write-host "# Purpose: Created to help organizations define their naming convention for Azure resources.         #"
    Write-Host "#                                                                                                    #"
    Write-Host "######################################################################################################"
    Write-Host ""
}

function Get-Shortnames
{
    param($Environment)
    switch($Environment)
    {
        Development {[pscustomobject][ordered]@{Name = 'Development'; Short = 'dev'; Shorter = 'dv'}}
        Test {[pscustomobject][ordered]@{Name = 'Test'; Short = 'tst'; Shorter = 'tt'}}
        Production {[pscustomobject][ordered]@{Name = 'Production'; Short = 'prod'; Shorter = 'pd'}}
        Sandbox {[pscustomobject][ordered]@{Name = 'Sandbox'; Short = 'sbx'; Shorter = 'sb'}}
        Staging {[pscustomobject][ordered]@{Name = 'Staging'; Short = 'stg'; Shorter = 'sg'}}
        Shared {[pscustomobject][ordered]@{Name = 'Shared'; Short = 'shd'; Shorter = 'sh'}}
        UAT {[pscustomobject][ordered]@{Name = 'UAT'; Short = 'uat'; Shorter = 'ut'}}
    }
}

function Test-NumberInput
{
    param (
        [parameter(Mandatory)]
        [array]$Answer,

        [parameter(Mandatory)]
        [bool]$SingleAnswer,
    
        [parameter(Mandatory=$false)]
        [int]$AnswerCount = 0
    
    )

    do 
    {
        $Response = Read-Host -Prompt "Response"
        Write-Host ""

        # TEST 01: Ensure the input only includes numbers
        $Test = $Response -as [Double]
        if($Test -eq $null) 
        {
            Write-Host "You must enter numeric values"
            Write-Host ""
        }
        
        # TEST 02: Ensure the number is not out-of-bounds
        if($Test -ne $null)
        {
            #Validating the numbers selected
            $Test2 = 0
            foreach($Character in $Response.ToCharArray())
            {
                [int]$Number = [string]$Character
                if($Number -lt 1 -or $Number -gt $Answer.Count)
                {
                    $Test2++ 
                }
            }
            if($Test2 -gt 0)
            {
                Write-Host "You must enter numeric values greater than 0 and less than $($Answer.Count + 1)"
                Write-Host ""
            }
        }

        # TEST 03: ensure a single number for single digit answers
        if($Test -ne $null -and $Test2 -eq 0)
        {
            if($SingleAnswer)
            {
                if($Response.Length -eq 1)
                {
                    $Test3 = $true
                }
                else
                {
                    Write-Host "You must enter a single number"
                    Write-Host ""
                    $Test3 = $false
                }
            }
            else
            {
                $Test3 = $true
            }
        }

        # TEST 04: ensure numbers are not repeated for multi digit answers
        if($Test -ne $null -and $Test2 -eq 0 -and $Test3 -eq $true)
        {
            #Validating the numbers selected
            $Test4 = 0
            $Test4Array = $Response.ToCharArray()
            foreach($Character in $Test4Array)
            {
                $MatchCount = ($Test4Array -match $Character).Count 
                if($MatchCount -gt 1)
                {
                    $Test4++ 
                }
            }
            if($Test4 -gt 0)
            {
                Write-Host "You must enter each number only one time"
                Write-Host ""
            }
        }

        # TEST 05: ensure all options are input
        if($Test -and $Test2 -eq 0 -and $Test3 -eq $true -and $Test4 -eq 0 -and $AnswerCount -gt 0)
        {
            # Validating the answer count
            $Test5Array = $Response.ToCharArray().Count
            if($Test5Array -ne $AnswerCount)
            {
                $Test5 = $false
                Write-Host "You must enter all of the options"
                Write-Host ""
            }
            else
            {
                $Test5 = $true
            }
        }
    }
    until(($Test -and $Test2 -eq 0 -and $Test3 -eq $true -and $Test4 -eq 0 -and $AnswerCount -eq 0) -or ($Test -and $Test2 -eq 0 -and $Test3 -eq $true -and $Test4 -eq 0 -and $Test5) -or $Response -eq "")
    return $Response
}

function Test-TextInput
{
    param (
        [parameter(Mandatory)]
        [int]$MaxLength,

        [parameter(Mandatory)]
        [ValidateSet("Name","Short")]
        [string]$Type,

        [parameter(Mandatory=$false)]
        [string]$Value
    )

    do 
    {
        # If $Value exists, throw an error if the value doesn't the naming requirements
        if($Value)
        {
            $Response = $Value

            # TEST 01: Ensure the input is less than or equal to the maximum length
            if($Response.Length -gt $MaxLength) 
            {
                Write-Host "Your data in the CSV file must be valid."
                Write-Host "Invalid value: $Value"
                Write-Host "The valid length is $MaxLength characters or less."
                Write-Host ""
                throw
            }

            # TEST 02: Ensure the input is alphanumeric
            if($Response -notmatch '^[a-zA-Z0-9 ]+$' -and $Type -eq 'Name')
            {
                Write-Host "Your data in the CSV file must be valid."
                Write-Host "Invalid value: $Value"
                Write-Host "The value must contain alphanumeric characters and spaces only."
                Write-Host ""
                throw
            }

            if($Response -notmatch '^[a-zA-Z0-9]+$' -and $Type -eq 'Short')
            {
                Write-Host "You must enter a value that only contains alphanumeric characters."
                Write-Host "Your data in the CSV file must be valid."
                Write-Host "Invalid value: $Value"
                Write-Host "The value must contain alphanumeric characters only."
                Write-Host ""
                throw
            }
        }
        # if $Value is null, loop until the input value is valid
        else
        {
            $Response = Read-Host -Prompt "Response"
            Write-Host ""

            # TEST 01: Ensure the input is less than or equal to the maximum length
            if($Response.Length -gt $MaxLength) 
            {
                Write-Host "You must enter a value with $MaxLength characters or less."
                Write-Host ""
            }

            # TEST 02: Ensure the input is alphanumeric
            if($Response -notmatch '^[a-zA-Z0-9 ]+$' -and $Type -eq 'Name')
            {
                Write-Host "You must enter a value that only contains alphanumeric characters and spaces."
                Write-Host ""
            }

            if($Response -notmatch '^[a-zA-Z0-9]+$' -and $Type -eq 'Short')
            {
                Write-Host "You must enter a value that only contains alphanumeric characters."
                Write-Host ""
            }
        }
    }
    until($Response.Length -le $MaxLength -and (($Response -match '^[a-zA-Z0-9 ]+$' -and $Type -eq 'Name') -or ($Response -match '^[a-zA-Z0-9]+$' -and $Type -eq 'Short')))
    
    if(!$Value)
    {
        return $Response
    }
}


#################################################################
# Variables
#################################################################
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

$Delimiters = @(
    '',
    '-',
    '.',
    '_'
)


#################################################################
# Step 01: Remove the unwanted naming components
#################################################################
$Import1 = (Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'Component'}).Name
do
{   
    Get-Header
    Write-Host "STEP 01: REMOVE THE UNDESIRED AZURE RESOURCE NAMING COMPONENTS"
    Write-Host ""
    if ($Import1)
    {
        #Validate import data
        foreach($Answer in $Import1)
        {
            
        }

        # Display previous input
        Write-Host "Previous Input:"
        Write-Host ""
        $Option = 0
        foreach($Answer in $Import1)
        {
            $Option++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "$($Answer)"
        }
        Write-Host ""
        do 
        {
            Write-Host "Do you want to keep these components? Y for Yes and N for No"
            Write-Host ""
            $Keep = Read-Host -Prompt 'Response'
            Write-Host ""
        }
        until ($Keep -eq 'Y' -or $Keep -eq 'N')
    }
    else
    {
        $Keep = 'N'
    }

    if ($Keep -eq 'Y')
    {
        $Answer1 = $Import1
    }
    elseif ($Keep -eq 'N')
    {
        Get-Header
        Write-Host "STEP 01: REMOVE THE UNDESIRED AZURE RESOURCE NAMING COMPONENTS"
        Write-Host ""
        Write-Host "The following components are mandatory and will not be removed if selected: ResourceType and Org. The Org component is only used for naming subscriptions and management groups. The VmRole component is only used to name resources associated with virtual machines and is excluded by default on all other Azure resources."
        Write-Host ""
        Write-Host "Using the number next to each component below, input the number for each UNDESIRED component(s) without spacing. For example, input 37 to remove UnitDept and Location, or simply press enter to keep all components."
        Write-Host ""
        $Answer1 = $Components
        $Option = 0
        foreach($Component in $Answer1)
        {
            $Option ++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "$($Component): " -NoNewline
            Write-Host "$(Get-Description -Component $Component)"
        }    
        # Collect input & create an array
        Write-Host ""
        $Q1 = Test-NumberInput -Answer $Answer1 -SingleAnswer $false
        $Remove = $Q1.ToCharArray()
        
        if($Remove)
        {
            $Values = @()
            foreach($Number in $Remove)
            {
                $Index = $Number.ToString() - 1
                $Values += $Answer1[$Index]
            }
            foreach($Value in $Values)
            {
                if($Value -ne 'Org' -and $Value -ne 'ResourceType')
                {
                    $Answer1 = $Answer1 -ne $Value
                }
            }
        }

        Get-Header
        Write-Host "STEP 01: REMOVE THE UNDESIRED AZURE RESOURCE NAMING COMPONENTS"
        Write-Host ""
        Write-Host "You have selected the following components:"
        Write-Host ""
        $Option = 0
        foreach($Answer in $Answer1)
        {
            $Option++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "$($Answer)"
        }
        Write-Host ""

        #Verify if input is accurate
        $YesOrNo = Confirm-Input
    }
}
Until ($YesOrNo -eq 'y' -or $Keep -eq 'Y')
$Components = $Answer1


#################################################################
# QUESTION 02: Order the desired naming components
#################################################################
if($Keep -eq 'Y')
{
    $Import2 = (Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'Component'}).Name
}
else
{
    $Import2 = $null
}

do
{   
    Get-Header
    Write-Host "STEP 02: ORDER YOUR NAMING COMPONENTS"
    Write-Host ""
    if ($Import2)
    {
        Write-Host "Previous Input:"
        Write-Host ""
        $Option = 0
        foreach($Answer in $Import2)
        {
            $Option++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "$($Answer)"
        }
        Write-Host ""
        do 
        {
            Write-Host "Do you want to keep this order? Y for Yes and N for No"
            Write-Host ""
            $Keep = Read-Host -Prompt 'Response'
            Write-Host ""
        }
        until ($Keep -eq 'Y' -or $Keep -eq 'N')
    }
    else
    {
        $Keep = 'N'
    }

    if ($Keep -eq 'Y')
    {
        $Answer2 = $Import2
    }
    elseif ($Keep -eq 'N')
    {
        Get-Header
        Write-Host "STEP 02: ORDER YOUR NAMING COMPONENTS"
        Write-Host ""
        Write-Host "Using the number next to each component below, input each number to determine your naming components order."
        Write-Host ""
        Write-Host "NOTE: Do not put the instance component first.  The name of many Azure resources cannot begin with a number."
        Write-Host ""
        $Answer2 = $Components
        $Option = 0
        foreach($Component in $Answer2)
        {
            $Option ++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "$($Component): " -NoNewline
            Write-Host "$(Get-Description -Component $Component)"
        }
    
        # Collect input & create an array
        Write-Host ""
        $Q2 = Test-NumberInput -Answer $Answer2 -SingleAnswer $false -AnswerCount $Answer2.Count
        $Order = $Q2.ToCharArray()

        # Update the components array if requested
        if($Order.Count -eq $Answer2.Count)
        {
            $Values = @()
            foreach($Number in $Order)
            {
                $Index = $Number.ToString() - 1
                $Values += $Answer2[$Index]
            }
            $Answer2 = $Values
        }

        #Verify Order of components
        Get-Header
        Write-Host "STEP 02: ORDER YOUR NAMING COMPONENTS"
        Write-Host ""
        Write-Host "You have selected the following order of components:"
        Write-Host ""
        $Option = 0
        foreach($Answer in $Answer2)
        {
            $Option ++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "$Answer"
        }
        Write-Host ""

        #Verify if input is accurate
        $YesOrNo = Confirm-Input
        Write-Host ""
    }
}
Until ($YesOrNo -eq 'y' -or $Keep -eq 'y')
$Components = $Answer2


#################################################################
# QUESTION 03: Delimiter between naming components
#################################################################
$Import3 = (Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'Delimiter'}).Name
do
{   
    Get-Header
    Write-Host "STEP 03: SELECT A DELIMITER FOR YOUR NAMING CONVENTION"
    Write-Host ""
    if ($Import3)
    {
        Write-Host "Previous Input:"
        Write-Host ""
        Write-Host $("    Delimiter: '" + $Import3 + "'")
        Write-Host ""
        Write-Host "    Example: <$($Components -join ">$Import3<")>"
        Write-Host ""
        Write-Host ""
        do 
        {
            Write-Host "Do you want to keep this delimiter? Y for Yes and N for No"
            Write-Host ""
            $Keep = Read-Host -Prompt 'Response'
            Write-Host ""
        }
        until ($Keep -eq 'Y' -or $Keep -eq 'N')
    }
    else
    {
        $Keep = 'N'
    }

    if ($Keep -eq 'Y')
    {
        $Output3 = $Import3
    }
    elseif ($Keep -eq 'N')
    {
        Get-Header
        Write-Host "STEP 03: SELECT A DELIMITER FOR YOUR NAMING CONVENTION"
        Write-Host ""
        Write-Host "Allowed options are no delimiters, dash (-), period (.), or underscore (_)."
        Write-Host "Please input prefered delimiter using the number listed next to the examples below."
        Write-Host ""
        $Answer3 = $Delimiters
        $Option = 0
        foreach($Delimiter in $Delimiters)
        {
            $Option ++
            Write-Host "    ($($Option)) " -NoNewline
            Write-Host "<$($Components -join ">$Delimiter<")>"
        }
        Write-Host ""

        # Collect input & create an array
        $Q3 = Test-NumberInput -Answer $Answer3 -SingleAnswer $true
        $Index = $Q3 - 1
        $Output3 = $Delimiters[$Index]

        #Verify selection
        Get-Header
        Write-Host "STEP 03: SELECT A DELIMITER FOR YOUR NAMING CONVENTION"
        Write-Host ""
        Write-Host "You have selected the following delimiter:"
        Write-Host ""
        Write-Host $("    Delimiter: '" + $Output3 + "'")
        Write-Host ""
        Write-Host "    Example: <$($Components -join ">$Output3<")>"
        Write-Host ""

        #Verify if input is accurate
        $YesOrNo = Confirm-Input
    }
}
Until ($YesOrNo -eq 'y' -or $Keep -eq 'y')


#################################################################
# QUESTION 04: Organization name and shortname
#################################################################
$Import4 = Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'Org'}
do
{
    
    Get-Header
    Write-Host "STEP 04: INPUT YOUR ORGANIZATION'S NAME"
    Write-Host ""
    if ($Import4)
    {
        Write-Host "Previous Input:"
        Write-Host ""
        Test-TextInput -MaxLength 25 -Type Name -Value $Import4.Name
        Test-TextInput -MaxLength 5 -Type Short -Value $Import4.Short
        Write-Host $("    Name: " + $Import4.Name + ", Shortname: " + $Import4.Short)
        Write-Host ""
        do 
        {
            Write-Host "Do you want to keep this Org name? Y for Yes and N for No"
            Write-Host ""
            $Keep = Read-Host -Prompt 'Response'
            Write-Host ""
        }
        until ($Keep -eq 'Y' -or $Keep -eq 'N')
    }
    else
    {
        $Keep = 'N'
    }

    if ($Keep -eq 'Y')
    {
        $Output4 = [pscustomobject][ordered]@{Name = $Import4.Name; Short = $Import4.Short}
    }
    elseif ($Keep -eq 'N')
    {
        Get-Header
        Write-Host "STEP 04: INPUT YOUR ORGANIZATION'S NAME"
        Write-Host ""
        Write-Host "Input the name of your organization, 25 character maximum."
        Write-Host ""
        $Q4Name = Test-TextInput -MaxLength 25 -Type Name
        Write-Host ""
        Write-Host "Input a short name for your organization, 5 character maximum"
        Write-Host ""
        $Q4Short = (Test-TextInput -MaxLength 5 -Type Short).toLower()
        Write-Host ""
        $Output4 = [pscustomobject][ordered]@{Name = $Q4Name; Short = $Q4Short}

        #Verify selection
        Get-Header
        Write-Host "STEP 04: INPUT YOUR ORGANIZATION'S NAME"
        Write-Host ""
        Write-Host "You have input the following organization:"
        Write-Host ""
        Write-Host "    Name: $($Output4.Name), Shortname: $($Output4.Short)"
        Write-Host ""

        #Verify if input is accurate
        $YesOrNo = Confirm-Input
    }
}
Until ($YesOrNo -eq 'y' -or $Keep -eq 'y')


#################################################################
# QUESTION 05: Business units and departments
#################################################################
if($Components -contains 'UnitDept')
{
    do
    {
        $Counter = 0 
        $Output5 = @()
        $Import5 = Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'UnitDept'}
        if($Import5)
        {
            foreach($Entry in $Import5)
            {
                $Output5 += [pscustomobject][ordered]@{Name = $Entry.Name; Short = $Entry.Short}
            }
        }

        do
        {
            Get-Header
            Write-Host "STEP 05: INPUT YOUR BUSINESS UNITS AND / OR DEPARTMENTS"
            Write-Host ""
            if ($Output5)
            {
                Write-Host "Previous Input:"
                Write-Host ""
                foreach($Entry in $Output5)
                {
                    Test-TextInput -MaxLength 25 -Type Name -Value $Entry.Name
                    Test-TextInput -MaxLength 3 -Type Short -Value $Entry.Short
                    Write-Host $("    Name: " + $Entry.Name + ", Shortname: " + $Entry.Short)
                }
                Write-Host ""
            }
            else
            {
                $Counter++
            }

            if ($Counter -gt 0)
            {
                Get-Header
                Write-Host "STEP 05: INPUT BUSINESS UNITS AND / OR DEPARTMENTS"
                Write-Host ""
                if($Output5)
                {
                    foreach($Entry in $Output5)
                    {
                        Write-Host $("    Name: " + $Entry.Name + ", Shortname: " + $Entry.Short)
                    }
                    Write-Host ""
                }
                Write-Host "Input a name of one of your business units or departments, 25 character maximum."
                Write-Host ""
                $Q5Name = Test-TextInput -MaxLength 25 -Type Name
                Write-Host ""
                Write-Host "Input a short name for the previously entered name, 3 character maximum."
                Write-Host ""
                $Q5Short = (Test-TextInput -MaxLength 3 -Type Short).toLower()
                Write-Host ""
                $Output5 += [pscustomobject][ordered]@{Name = $Q5Name; Short = $Q5Short}
            }
            Write-Host "Do you want to add more? Y for Yes and N for No"
            Write-Host ""
            do
            {
                $More = Read-Host -Prompt 'Response'
                if($More -ne 'n' -and $More -ne 'y')
                {
                    Write-Host ""
                    Write-Host "An incorrect value was entered.  Please enter either 'n' or 'y'."
                    Write-Host ""
                }
            }
            until ($More -eq 'n' -or $More -eq 'y')
            Write-Host ""
            Write-Host ""
            $Counter++
        }
        until($More -eq 'n')

        if($Counter -gt 1)
        {
            #Verify selection
            Get-Header
            Write-Host "STEP 05: INPUT YOUR BUSINESS UNITS AND / OR DEPARTMENTS"
            Write-Host ""
            Write-Host "You have input the following business units and / or departments:"
            Write-Host ""
            foreach($Entry in $Output5)
            {
                Write-Host $("    Name: " + $Entry.Name + ", Shortname: " + $Entry.Short)
            }
            Write-Host ""
            #Verify if input is accurate
            $YesOrNo = Confirm-Input
        }
    }
    Until ($YesOrNo -eq 'y' -or $Counter -eq 1)
}


#################################################################
# QUESTION 06: Projects, Applications, and / or Services
#################################################################
if($Components -contains 'ProjAppSvc')
{
    do
    {
        $Counter = 0 
        $Output6 = @()
        $Import6 = Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'ProjAppSvc'}
        if($Import6)
        {
            foreach($Entry in $Import6)
            {
                $Output6 += [pscustomobject][ordered]@{Name = $Entry.Name; Short = $Entry.Short}
            }
        }

        do
        {
            Get-Header
            Write-Host "STEP 06: INPUT YOUR PROJECTS, APPLICATIONS, AND / OR SERVICES"
            Write-Host ""
            if ($Output6)
            {
                Write-Host "Previous Input:"
                Write-Host ""
                foreach($Entry in $Output6)
                {
                    Test-TextInput -MaxLength 25 -Type Name -Value $Entry.Name
                    Test-TextInput -MaxLength 3 -Type Short -Value $Entry.Short
                    Write-Host $("    Name: " + $Entry.Name + ", Shortname: " + $Entry.Short)
                }
                Write-Host ""
            }
            else
            {
                $Counter++
            }

            if ($Counter -gt 0)
            {
                Get-Header
                Write-Host "STEP 06: INPUT YOUR PROJECTS, APPLICATIONS, AND / OR SERVICES"
                Write-Host ""
                if($Output6)
                {
                    foreach($Entry in $Output6)
                    {
                        Write-Host $("    Name: " + $Entry.Name + ", Shortname: " + $Entry.Short)
                    }
                    Write-Host ""
                }
                Write-Host "Input a name of one of your projects, applications, or services, 25 character maximum."
                Write-Host ""
                $Q6Name = Test-TextInput -MaxLength 25 -Type Name
                Write-Host ""
                Write-Host "Input a short name for the previously entered name, 3 character maximum."
                Write-Host ""
                $Q6Short = (Test-TextInput -MaxLength 3 -Type Short).toLower()
                Write-Host ""
                $Output6 += [pscustomobject][ordered]@{Name = $Q6Name; Short = $Q6Short}
            }
            Write-Host "Do you want to add more? Y for Yes and N for No"
            Write-Host ""
            $More = Read-Host -Prompt 'Response'
            Write-Host ""
            Write-Host ""
            $Counter++
        }
        until($More -eq 'n')

        if($Counter -gt 1)
        {
            #Verify selection
            Get-Header
            Write-Host "STEP 06: INPUT YOUR PROJECTS, APPLICATIONS, AND / OR SERVICES"
            Write-Host ""
            Write-Host "You have input the following projects, applications, and / or services."
            Write-Host ""
            foreach($Entry in $Output6)
            {
                Write-Host $("    Name: " + $Entry.Name + ", Shortname: " + $Entry.Short)
            }
            Write-Host ""
            #Verify if input is accurate
            $YesOrNo = Confirm-Input
        }
    }
    Until ($YesOrNo -eq 'y' -or $Counter -eq 1)
}


#################################################################
# QUESTION 07: Environments
#################################################################
if($Components -contains 'Environment')
{
    $Counter = 0 
    $Output7 = @()
    $Import7 = Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'Environment'}
    if($Import7)
    {
        foreach($Entry in $Import7)
        {
            $Output7 += [pscustomobject][ordered]@{Name = $Entry.Name; Short = $Entry.Short}
        }
    }

    do
    {
        if ($Import7)
        {
            Get-Header
            Write-Host "STEP 07: REMOVE THE UNDESIRED ENVIRONMENTS"
            Write-Host ""
            Write-Host "Previous Input:"
            Write-Host ""
            $Option = 0
            foreach($Answer in $Import7)
            {
                Write-Host "    $($Answer.Name)"
            }
            Write-Host ""
            do 
            {
                Write-Host "Do you want to keep these environments? Y for Yes and N for No"
                Write-Host "By selecting 'No', you will be able to add or remove values."
                Write-Host ""
                $Keep = Read-Host -Prompt 'Response'
                Write-Host ""
            }
            until ($Keep -eq 'Y' -or $Keep -eq 'N')
        }
        else
        {
            $Keep = 'N'
        }

        if ($Keep -eq 'Y')
        {
            $Answer7 = $Import7
        }
        elseif ($Keep -eq 'N')
        {
            Get-Header
            Write-Host "STEP 07: REMOVE THE UNDESIRED ENVIRONMENTS"
            Write-Host ""
            Write-Host "Using the number next to each environment type below, input the number for each UNDESIRED environment(s) without spacing."
            Write-Host "For example, input '47' to remove Sandbox and UAT, or simply press enter to keep all components."
            Write-Host ""
            $Environments = Import-Csv -Path '.\data\environments.csv' -ErrorAction Stop
            $Answer7 = $Environments
            $Option = 0
            foreach($Environment in $Environments)
            {
                $Option ++
                Write-Host "    ($($Option)) " -NoNewline
                Write-Host "$($Environment.Name) " -NoNewline
                Write-Host "$(Get-Description -Component $Environments)"
            }    
            # Collect input & create an array
            Write-Host ""

            $Q7 = Test-NumberInput -Answer $Answer7 -SingleAnswer $false
            $Remove = $Q7.ToCharArray()
        
            if($Remove)
            {
                $Values = @()
                foreach($Number in $Remove)
                {
                    $Index = $Number.ToString() - 1
                    $Values += $Answer7[$Index]
                }
                foreach($Value in $Values)
                {
                    $Answer7 = $Answer7 -ne $Value
                }
            }
        
            Get-Header
            Write-Host "STEP 07: REMOVE THE UNDESIRED ENVIRONMENTS"
            Write-Host ""
            Write-Host "You have selected the following environments:"
            Write-Host ""
            foreach($Answer in $Answer7)
            {
                Write-Host "    $($Answer.Name)"
            }
            Write-Host ""

            #Verify if input is accurate
            $YesOrNo = Confirm-Input
        }
    }
    Until ($YesOrNo -eq 'y' -or $Keep -eq 'y')
}


#################################################################
# QUESTION 08: Clouds
#################################################################
if($Components -contains 'Location')
{
    $Counter = 0 
    $Output8 = @()
    $Import8 = Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'Clouds'}
    $Locations = Import-Csv -Path '.\data\locations.csv' -ErrorAction Stop
    if($Import8)
    {
        foreach($Entry in $Import8)
        {
            $Output8 += [pscustomobject][ordered]@{Name = $Entry.Name}
        }
    }

    do
    {
        if ($Import8)
        {
            Get-Header
            Write-Host "STEP 08: REMOVE THE UNDESIRED AZURE CLOUD ENVIRONMENTS TO DETERMINE YOUR AZURE LOCATIONS"
            Write-Host ""
            Write-Host "Previous Input:"
            Write-Host ""
            $Option = 0
            foreach($Answer in $Import8)
            {
                Write-Host "    $($Answer.Name)"
            }
            Write-Host ""
            do 
            {
                Write-Host "Do you want to keep these Clouds? Y for Yes and N for No"
                Write-Host ""
                $Keep = Read-Host -Prompt 'Response'
                Write-Host ""
            }
            until ($Keep -eq 'Y' -or $Keep -eq 'N')
        }
        else
        {
            $Keep = 'N'
        }

        if ($Keep -eq 'Y')
        {
            $Answer8 = $Import8.Name
        }
        elseif ($Keep -eq 'N')
        {
            Get-Header
            Write-Host "STEP 08: REMOVE THE UNDESIRED AZURE CLOUD ENVIRONMENTS TO DETERMINE YOUR AZURE LOCATIONS"
            Write-Host ""
            Write-Host "Using the number next to each Cloud below, input the number for each UNDESIRED Azure Cloud without spacing."
            Write-Host ""
            $Answer8 = $Locations | Select-Object -ExpandProperty Cloud -Unique
            $Option = 0
            foreach($Cloud in $Answer8)
            {
                $Option ++
                Write-Host "    ($($Option)) " -NoNewline
                Write-Host "$Cloud "
            }    
            # Collect input & create an array
            Write-Host ""
            $Q8 = Test-NumberInput -Answer $Answer8 -SingleAnswer $false
            $Remove = $Q8.ToCharArray()
        
            if($Remove)
            {
                $Values = @()
                foreach($Number in $Remove)
                {
                    $Index = $Number.ToString() - 1
                    $Values += $Answer8[$Index]
                }
                foreach($Value in $Values)
                {
                    $Answer8 = $Answer8 -ne $Value
                }
            }
        
            Get-Header
            Write-Host "STEP 08: REMOVE THE UNDESIRED AZURE CLOUD ENVIRONMENTS TO DETERMINE YOUR AZURE LOCATIONS"
            Write-Host ""
            Write-Host "You have selected the following Azure Clouds:"
            Write-Host ""
            foreach($Answer in $Answer8)
            {
                Write-Host "    $($Answer)"
            }
            Write-Host ""

            #Verify if input is accurate
            $YesOrNo = Confirm-Input
        }
    }
    Until ($YesOrNo -eq 'y' -or $Keep -eq 'y')
}


#################################################################
# QUESTION 09: Geography Groups
#################################################################
if($Components -contains 'Location')
{

    $Counter = 0 
    $Output9 = @()
    $Import9 = Import-Csv -Path '.\configuration.csv' -ErrorAction SilentlyContinue | Where-Object {$_.Question -eq 'GeoGroups'}
    if($Import9)
    {
        foreach($Entry in $Import9)
        {
            $Output9 += [pscustomobject][ordered]@{Name = $Entry.Name}
        }
    }
    do
    {
        if ($Import9)
        {
            Get-Header
            Write-Host "STEP 09: REMOVE THE UNDESIRED AZURE GEOGRAPHY GROUPS TO DETERMINE YOUR AZURE LOCATIONS"
            Write-Host ""
            Write-Host "Previous Input:"
            Write-Host ""
            $Option = 0
            foreach($Answer in $Import9)
            {
                Write-Host "    $($Answer.Name)"
            }
            Write-Host ""
            do 
            {
                Write-Host "Do you want to keep these Geography Groups? Y for Yes and N for No"
                Write-Host ""
                $Keep = Read-Host -Prompt 'Response'
                Write-Host ""
            }
            until ($Keep -eq 'Y' -or $Keep -eq 'N')
        }
        else
        {
            $Keep = 'N'
        }

        if ($Keep -eq 'Y')
        {
            $Answer9 = $Import9.Name
        }
        elseif ($Keep -eq 'N')
        {
            Get-Header
            Write-Host "STEP 09: REMOVE THE UNDESIRED AZURE GEOGRAPHY GROUPS TO DETERMINE YOUR AZURE LOCATIONS"
            Write-Host ""
            Write-Host "Using the number next to each Geogrpahy Group above, input the number for each UNDESIRED Azure Geography Group without spacing."
            Write-Host ""
            $Answer9 = $Locations | Where-Object {$Answer8 -contains $_.Cloud}
            $Answer9 = $Answer9 | Select-Object -ExpandProperty GeographyGroup -Unique
            $Option = 0
            foreach($Geo in $Answer9)
            {
                $Option ++
                Write-Host "    ($($Option)) " -NoNewline
                Write-Host "$Geo "
            }    
            # Collect input & create an array
            Write-Host ""
            $Q9 = Test-NumberInput -Answer $Answer9 -SingleAnswer $false
            $Remove = $Q9.ToCharArray()
        
            if($Remove)
            {
                $Values = @()
                foreach($Number in $Remove)
                {
                    $Index = $Number.ToString() - 1
                    $Values += $Answer9[$Index]
                }
                foreach($Value in $Values)
                {
                    $Answer9 = $Answer9 -ne $Value
                }
            }
            Get-Header
            Write-Host "STEP 09: REMOVE THE UNDESIRED AZURE GEOGRAPHY GROUPS TO DETERMINE YOUR AZURE LOCATIONS"
            Write-Host ""
            Write-Host "You have selected the following Azure Geography Groups:"
            Write-Host ""
            foreach($Answer in $Answer9)
            {
                Write-Host "    $($Answer)"
            }
            Write-Host ""

            #Verify if input is accurate
            $YesOrNo = Confirm-Input
        }
    }
    Until ($YesOrNo -eq 'y' -or $Keep -eq 'y')
}


#################################################################
# Output configuration to CSV file
#################################################################
$Configuration = @()
foreach($Component in $Components)
{
    $Configuration += [pscustomobject][ordered]@{
        Question = 'Component'
        Name = $Component
        Short = $Component
    }
}
$Configuration += [pscustomobject][ordered]@{
    Question = 'Delimiter'
    Name = $output3
    Short = $output3
}
$Configuration += [pscustomobject][ordered]@{
    Question = 'Org'
    Name = $output4.Name
    Short = $output4.Short
}
if($Components -contains 'UnitDept')
{
    foreach($Output in $Output5)
    {
        $Configuration += [pscustomobject][ordered]@{
            Question = 'UnitDept'
            Name = $Output.Name
            Short = $Output.Short
        }
    }
}
if($Components -contains 'ProjAppSvc')
{
    foreach($Output in $Output6)
    {
        $Configuration += [pscustomobject][ordered]@{
            Question = 'ProjAppSvc'
            Name = $Output.Name
            Short = $Output.Short
        }
    }
}
if($Components -contains 'Environment')
{
    foreach($Output in $Answer7)
    {
        $Configuration += [pscustomobject][ordered]@{
            Question = 'Environment'
            Name = $Output.Name
            Short = $Output.Short
        }
    }
}
if($Components -contains 'Location')
{

    foreach($Output in $Answer8)
    {
        $Configuration += [pscustomobject][ordered]@{
            Question = 'Clouds'
            Name = $Output
            Short = $Output
        }
    }

    foreach($Output in $Answer9)
    {
        $Configuration += [pscustomobject][ordered]@{
            Question = 'GeoGroups'
            Name = $Output
            Short = $Output
        }
    }

}

$Configuration | Export-Csv -NoTypeInformation -Path '.\configuration.csv' -Force -ErrorAction Stop


##################################################################
# Prepare data for HTML file
##################################################################
$Resources = Import-Csv '.\data\resources.csv' -ErrorAction Stop | Sort-Object resource
$VmRoles = Import-Csv '.\data\vmRoles.csv' -ErrorAction Stop | Sort-Object Name
$Location = $Locations | Where-Object {$Answer8 -contains $_.Cloud -and $Answer9 -contains $_.GeographyGroup}

$Object = New-Object PSObject
foreach($Component in $Components)
{
    if($Component -eq 'Environment'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Answer7
    }
    elseif($Component -eq 'Instance'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value "001"
    }
    elseif($Component -eq 'Location'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $($Location | Sort-Object Name | Select-Object -Property Name,Short)
    }
    elseif($Component -eq 'Org'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Output4
    }
    elseif($Component -eq 'ProjAppSvc'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Output6
    }
    elseif($Component -eq 'ResourceType'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Resources
    }
    elseif($Component -eq 'UnitDept'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $Output5
    }
    elseif($Component -eq 'VmRole'){
        $Object | Add-Member -MemberType NoteProperty -Name $Component -Value $VmRoles
    }
}

$ObjectJson = $([System.Text.RegularExpressions.Regex]::Unescape($($Object | ConvertTo-Json -Depth 100)))


##################################################################
# Create HTML file
################################################################## 

$Html = @'
<!DOCTYPE html>
<html>

<head>
    <title>Azure Naming Tool for {organization}</title>
    <meta charset="UTF-8">
    <meta name="description" content="Azure Naming Tool">
    <meta name="keywords" content="Azure, Governance, Naming, Convention, Reference, Generator">
    <meta name="author" content="FastTrack for Azure">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="icon" type="image/x-icon" href="https://portal.azure.com/favicon.ico">
    <style>
        * {
            font-family: Arial, Helvetica, sans-serif;
            color: black;
        }

        html,
        body {
            height: 100%;
        }

        a {
            color: #925800;
        }

        body {
            display: flex;
            flex-direction: column;
            margin: 0;
        }

        .button,
        .tooltip button {
            background-color: #ff9900;
            background-image: linear-gradient(180deg, #ffc670, #ff9900, #9b5d00);
            border: 1px solid black;
            color: black;
            height: auto;
            padding: 10px;
        }

        .button:hover,
        .tooltip button:hover {
            background-color: #ffa622;
            background-image: linear-gradient(180deg, #ffc670, #ff9900);
        }

        .form .button {
            margin: 0 20px 0 0;
        }

        #choice {
            clear: both;
            overflow: auto;
        }

        #choice label {
            padding: 10px 0
        }

        #choice label,
        #choice select,
        #choice input {
            clear: none;
            float: left;
            margin: 0 15px 0 0;
        }

        .config {
            width: auto;
            float: left;
            clear: none;
            margin: 0 20px 20px 0;
            padding: 0;
        }

        .config h4 {
            margin-top: 0;
        }

        .config table {
            width: auto;
        }

        #displayName {
            font-size: 18px;
        }

        #displayValidation li,
        #displayValidation strong {
            color: red;
        }

        .example {
            font-size: 20px;
            font-weight: bold;
        }

        footer {
            background-color: rgba(128, 128, 128);
            padding: 20px;
        }

        footer a {
            color: #ffae34;
        }

        footer p {
            color: white;
            margin: 0;
        }

        .form {
            margin: 10px;
            padding: 10px;
        }

        #configuration,
        #generator,
        #reference {
            display: none;
        }

        header {
            padding: 0 20px;
        }

        h1 {
            color: #0078d4;
        }

        h3 {
            clear: both;
        }

        input,
        select {
            border: 1px solid black;
            font-size: 14px;
            padding: 10px;
            width: auto;
        }

        label {
            display: block;
            font-size: 14px;
            padding-bottom: 10px;
        }

        input.data {
            display: block;
            width: auto;
        }

        main {
            background-color: lightgray;
            flex-grow: 1;
            padding: 0 20px 20px 20px;
        }

        #name {
            display: inline-block;
            padding-right: 10px;
        }

        nav {
            background-color: #fff;
            background-image: linear-gradient(180deg, #fff, rgba(128, 128, 128, .25));
            border-bottom: rgb(128, 128, 128) solid 1px;
            padding: 0 20px;
        }

        nav a {
            border: rgb(128, 128, 128) solid 1px;
            border-top-left-radius: 10px;
            border-top-right-radius: 10px;
            display: inline-block;
            padding: 10px;
            margin: 0;
            background-color: #ff9900;
            background-image: linear-gradient(180deg, #ffc670, #ff9900, #9b5d00);
            color: black;
            text-decoration: none;
            position: relative;
            top: 1px;
        }

        nav a:hover {
            background-color: #ffa622;
            background-image: linear-gradient(180deg, #ffc670, #ff9900);
        }

        nav a.current,
        nav a.current:hover {
            background-color: lightgray;
            background-image: none;
            border-bottom: lightgray solid 1px;
        }

        p {
            color: black;
        }

        .optional,
        .readonly {
            color: rgb(68, 68, 68);
        }

        .required,
        .warning strong,
        .warning li {
            color: red;
        }

        #resources {
            width: 100%;
        }

        #resources td,
        #resources th {
            word-break: break-word;
            width: 33%;
        }

        #search {
            width: 30%;
            margin-bottom: 12px;
        }

        select.data {
            width: auto;
        }

        table {
            border: 1px lightgray solid;
            border-collapse: collapse;
            border-spacing: 0px;
            margin-bottom: 20px;
        }

        th,
        td {
            border: 3px lightgray solid;
            border-collapse: collapse;
            padding: 5px;
        }

        th {
            text-align: left;
            background-color: #0078d4;
            color: #ffffff;
            position: sticky;
            top: 0;
        }

        tr:nth-child(even) {
            background-color: #eaf5ff;
        }

        tr:nth-child(odd) {
            background-color: #fff;
        }

        .type {
            font-weight: bold;
        }
    </style>
</head>

<body>
    <header>
        <h1>Azure Naming Tool for {organization}</h1>
    </header>
    <nav id="nav">
        <a id="infoLink" class="current" href="#" onclick="displayDiv('info')">Information</a>
        <a id="configurationLink" href="#" onclick="displayDiv('configuration')">Configuration</a>
        <a id="referenceLink" href="#" onclick="displayDiv('reference')">Reference</a>
        <a id="generatorLink" href="#" onclick="displayDiv('generator')">Generate</a>
    </nav>
    <main>
        <div id="info">
            <h2>Information</h2>
            <h3>Disclaimer</h3>
            <p>
                This Naming Tool was developed using a naming pattern based on <a
                    href="https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/naming-and-tagging">Microsoft's
                    best
                    practices</a>, and uses a PowerShell script to define your organization’s preferred naming
                configuration. Once the organizational components have been defined, CSV files are used to further
                define the values for Azure naming components. These values are imported and hard coded into a
                JavaScript object for reference documentation.
            </p>
            <p>
                If you need to modify any of the values, you must modify only the CSV files, and then re-run the
                PowerShell script. This will recreate the HTML file for reference.
            </p>
            <p>
                Further documentation on the script can be found on <a href="">Microsoft's GitHub repo</a>.
            </p>

            <h3>Configuration</h3>
            <p>
                The Configuration tab provides data collected from the PowerShell script. Some of this data is static,
                while some of it was input based on your organization’s responses while running the configuration
                script. These values are all surfaced in the Generator tool, and can be used to create a custom name for
                an Azure resource.
            </p>

            <h3>References</h3>
            <p>
                The References tab provides examples for each type of Azure resource. The example values do not include
                any excluded naming components. Optional components are always displayed and are identified below the
                example . Since unique names are only required at specific scopes, the examples provided are only
                generated for the scopes above the resource scope: resource group, resource group & region, region,
                global, subscription, and tenant.
            </p>

            <h3>Generator</h3>
            <p>
                The Generator tab provides a drop down menu to select an Azure resource. Once a resource is selected,
                naming component options are provided. Read-only components cannot be changed, like the value for a
                resource type or organization. Optional components, if left blank, will be null and not shown in the
                output. Required components do not allow a null value, and the first value in the array is set as the
                default.
            </p>
        </div>
        <div id="configuration">
            <h2>Naming Configuration</h2>
            <p class="subnav">
                <a href="#componentsHeader">Components</a> |
                <a href="#environmentsHeader">Environments</a> |
                <a href="#locationsHeader">Locations</a> |
                <a href="#unitDeptHeader">Units & Departments</a> |
                <a href="#projAppSvcHeader">Projects, Applications, & Services</a> |
                <a href="#vmRolesHeader">Virtual Machine Roles</a>
            </p>
            <h3 id="componentsHeader">Components</h3>
            <table id="compConfig">
                <tr>
                    <th>Order</th>
                    <th>Components</th>
                </tr>
            </table>

            <h3 id="environmentsHeader">Environments</h3>
            <table id="Environment">
                <tr>
                    <th>Name</th>
                    <th>Short Name</th>
                </tr>
            </table>

            <h3 id="locationsHeader">Locations</h3>
            <table id="Location">
                <tr>
                    <th>Name</th>
                    <th>Short Name</th>
                </tr>
            </table>

            <h3 id="unitDeptHeader">Units & Departments</h3>
            <table id="UnitDept">
                <tr>
                    <th>Name</th>
                    <th>Short Name</th>
                </tr>
            </table>

            <h3 id="projAppSvcHeader">Projects, Applications, & Services</h3>
            <table id="ProjAppSvc">
                <tr>
                    <th>Name</th>
                    <th>Short Name</th>
                </tr>
            </table>

            <h3 id="vmRolesHeader">Virtual Machine Roles</h3>
            <table id="VmRole">
                <tr>
                    <th>Name</th>
                    <th>Short Name</th>
                </tr>
            </table>

        </div>
        <div id="reference">
            <h2>Naming Reference</h2>

            <form action="" method="POST" onsubmit="return false;">
                <input type="text" id="search" onkeyup="resourceSearch()" placeholder="Search for Azure Resources..">
            </form>

            <table id="resources">
                <tr>
                    <th>Resource</th>
                    <th>Example</th>
                    <th>Details</th>
                </tr>
            </table>

        </div>
        <div id="generator">
            <h2>Generate Name</h2>
            <div id="choice">
                <label for="rsrc">Choose the Azure Resource:</label>
                <select id="rsrc" name="rsrc" onchange="getResource()">
                    <option value=""></option>
                </select>
            </div>
            <div id="gnrtr"></div>
        </div>
    </main>
    <footer>
        <p>Questions or Concerns? Visit <a href="">Microsoft's GitHub repo</a></p>
    </footer>

    <script type="text/javascript">
        const scopes = ["resource group", "resource group & region", "region", "global", "subscription", "tenant"];
        const delimiter = "{delimiter}";
        const components = {components};         
        
        // Get Naming Components for a Resource
        function getResourceComponents(resource) {
            let resourceComponents = Object.keys(components);
            if (resource.exclude) {
                let resourceExclusions = resource.exclude.split(",");
                for (let i = 0; i < resourceExclusions.length; i++) {
                    resourceComponents = resourceComponents.filter(item => item !== resourceExclusions[i]);
                }
            }
            return resourceComponents;
        }

        // Creates a Naming form based on the resource selection
        function getResource() {

            // Clears the inner html
            document.getElementById("gnrtr").innerHTML = "";

            // Get resource name from drop down menu
            let form = document.getElementById("rsrc").value.split(", ");

            // Get resource data based on selection
            let resource = new Object;
            if (form[1]) {
                resource = components.ResourceType.filter(function (resource, i) {
                    return (resource["resource"] == form[0] && resource["property"] == form[1]);
                })[0]
            } else {
                resource = components.ResourceType.find(({ resource }) => resource === form[0]);
            }

            // Display H3 for resource
            let header = document.createElement("h3");
            header.id = "header";
            if (resource.property) {
                header.innerHTML = resource.resource + " (" + resource.property + ")";
            } else {
                header.innerHTML = resource.resource;
            }
            let gnrtr = document.getElementById("gnrtr");
            gnrtr.appendChild(header);

            // Remove excluded components
            let chosenComponents = getResourceComponents(resource);

            let optional = resource.optional.split(",");
            for (let i = 0; i < chosenComponents.length; i++) {
                let component = chosenComponents[i];
                let div = document.createElement("div");
                div.className = "form";
                let label = document.createElement("label");
                let index = optional.indexOf(component);
                if (index >= 0) {
                    label.innerHTML = '<strong>' + component + '</strong> <span class="optional">(Optional)</span>';
                } else if (component === "ResourceType" || component === "Org" || component === "Instance") {
                    label.innerHTML = '<strong>' + component + '</strong> <span class="readonly">(Read Only)</span>';
                } else {
                    label.innerHTML = '<strong>' + component + '</strong> <span class="required">(Required)</span>';
                }
                div.appendChild(label);

                if (component === "ResourceType") {

                    let input = document.createElement("input");
                    input.className = "data";
                    let form = document.getElementById("rsrc").value;
                    input.value = resource.shortName;
                    if (index >= 0) {
                        input.readOnly = false;
                    } else {
                        input.readOnly = true;
                    }
                    div.appendChild(input);

                } else if (component === "Org" && index === -1) {

                    let input = document.createElement("input");
                    input.className = "data";
                    let form = document.getElementById("rsrc").value;
                    input.value = components.Org.Short;
                    input.readOnly = true;
                    div.appendChild(input);

                } else if (component === "Instance" && index === -1) {

                    let input = document.createElement("input");
                    input.className = "data";
                    let form = document.getElementById("rsrc").value;
                    input.value = components.Instance;
                    input.readOnly = true;
                    div.appendChild(input);

                } else {

                    let select = document.createElement("select");
                    select.className = "data";

                    // Create empty option if component is Optional
                    if (index >= 0) {
                        let el = document.createElement("option");
                        el.text = "";
                        el.value = "";
                        select.add(el);
                    }

                    if (component === "Org") {

                        let value = components[component];
                        let el = document.createElement("option");
                        el.text = value.Name + " (" + value.Short + ")";
                        el.value = value.Short;
                        select.add(el);

                    } else if (component === "Instance") {

                        let value = components[component];
                        let el = document.createElement("option");
                        el.text = value;
                        el.value = value;
                        select.add(el);

                    } else {

                        for (let i = 0; i < components[component].length; i++) {
                            let value = components[component][i];
                            let el = document.createElement("option");
                            el.text = value.Name + " (" + value.Short + ")";
                            el.value = value.Short;
                            select.add(el);
                        }

                    }
                    div.appendChild(select);

                }

                let element = document.getElementById("gnrtr");
                element.appendChild(div);
            }

            // create Generate button
            let div = document.createElement("div");
            div.className = "form";
            let button = document.createElement("button");
            button.className = "button";
            button.id = "generateButton";
            button.type = "submit";
            button.innerText = "Generate & Copy to Clipboard";
            button.onclick = function () { generateName(); copyName() };
            div.appendChild(button);
            let element = document.getElementById("gnrtr");
            element.appendChild(div);

            // create display element for name
            let name = document.createElement("div");
            name.id = "displayName";
            name.innerHTML = "<p><strong>Name:</strong> (Select values and click the Generate button)</p>";
            element.appendChild(name);

            // create display element for validation
            let validation = document.createElement("div");
            validation.id = "displayValidation";
            element.appendChild(validation);
        }

        // Validate Name and output any errors
        function validateName(name, resource) {
            let output = document.createElement("ul");
            output.className = "warning";

            // Check RegEx
            if (name.match(resource.regx) === null) {
                let regex = document.createElement("li");
                regex.innerHTML = "The name does not match the regular expression for the resource. Regular Expression: " + resource.regx;
                output.appendChild(regex);
            }

            // Check Minimum Length
            if (name.length < resource.lengthMin) {
                let lengthMinOutput = document.createElement("li");
                lengthMinOutput.innerHTML = "The name length is too short. Current length: " + name.length + " characters; Allowed minimum length: " + resource.lengthMin + " characters";
                output.appendChild(lengthMinOutput);
            }

            // Check Maximum Length
            if (name.length > resource.lengthMax) {
                let lengthMaxOutput = document.createElement("li");
                lengthMaxOutput.innerHTML = "The name length is too long. Current length: " + name.length + " characters; Allowed maximum length: " + resource.lengthMax + " characters";
                output.appendChild(lengthMaxOutput);
            }

            // Check Invalid Characters
            let invalidChars = resource.invalidCharacters;
            for (let i = 0; i < invalidChars.length; i++) {
                if (name.includes(invalidChars[i])) {
                    let invalidCharsOutput = document.createElement("li");
                    invalidCharsOutput.innerHTML = "The name contains an invalid character: " + invalidChars[i];
                    output.appendChild(invalidCharsOutput);
                }
            }

            // Check Invalid Beginning Characters
            let invalidCharsBegin = resource.invalidCharactersStart;
            for (let i = 0; i < invalidCharsBegin.length; i++) {
                if (name.substring(0, 1) === invalidCharsBegin[i]) {
                    let invalidCharsBeginOutput = document.createElement("li");
                    invalidCharsBeginOutput.innerHTML = "The name begins with an invalid character: " + invalidCharsBegin[i];
                    output.appendChild(invalidCharsBeginOutput);
                }
            }

            // Check Invalid Ending Characters
            let invalidCharsEnd = resource.invalidCharactersEnd;
            for (let i = 0; i < invalidCharsEnd.length; i++) {
                if (name[name.length - 1] === invalidCharsEnd[i]) {
                    let invalidCharsEndOutput = document.createElement("li");
                    invalidCharsEndOutput.innerHTML = "The name ends with an invalid character: " + invalidCharsEnd[i];
                    output.appendChild(invalidCharsEndOutput);
                }
            }

            // Check Invalid Consecutive Characters
            let invalidCharsConsec = resource.invalidCharactersConsecutive;
            for (let i = 0; i < invalidCharsConsec.length; i++) {
                let consec = invalidCharsConsec[i] + invalidCharsConsec[i];
                if (name.includes(consec)) {
                    let invalidCharsConsecOutput = document.createElement("li");
                    invalidCharsConsecOutput.innerHTML = "The name has invalid consecutive characters: " + invalidCharsConsec[i];
                    output.appendChild(invalidCharsConsecOutput);
                }
            }
            return output;
        }

        // Validate Delimiter should be used
        function validateDelimiter(name, resource, delimiter) {
            let output = false;

            if (resource.regx !== undefined) {
                // remove quantifier on original regex since we only want to 
                // remove the delimiter if its an invalid character or not 
                // included in the valid characters regex
                let regx = resource.regx;
                let delimitRegex = regx.replace(/]{.+}/g, "]+");

                // Check RegEx
                if (name.match(delimitRegex) === null) {
                    output = true;
                }
            }

            // Check Invalid Characters
            let invalidChars = resource.invalidCharacters;
            if (invalidChars.includes(delimiter)) {
                output = true;
            }

            return output;
        }

        // Create Resource Example using variable data for the Example in the Resources table in the Reference div
        function generateExample(resource) {
            let output = document.createElement("div");
            let name = "";
            let delimitValidation = "";
            let componentsList = document.createElement("ul");

            //Creates Name from Static Values property if exists
            if (resource.staticValues) {
                name = resource.staticValues;

                // Creates Name based on allowed components
            } else if (scopes.indexOf(resource.scope) >= 0) {
                let exampleComponents = getResourceComponents(resource);
                let data = new Array();
                let compName = "";
                for (let i = 0; i < exampleComponents.length; i++) {
                    let value = "";
                    let componentsListItem = document.createElement("li");

                    let optional = resource.optional.split(",");
                    let index = optional.indexOf(exampleComponents[i]);
                    let note = "";
                    if (index >= 0) {
                        note = "(Optional)"
                    }

                    if (exampleComponents[i] === "UnitDept" || exampleComponents[i] === "ProjAppSvc" || exampleComponents[i] === "VmRole" || exampleComponents[i] === "Environment" || exampleComponents[i] === "Location") {
                        value = components[exampleComponents[i]][0].Short;
                        compName = components[exampleComponents[i]][0].Name;
                        componentsListItem.innerHTML = exampleComponents[i] + " " + note + ": " + compName + " (" + value + ")";
                    } else if (exampleComponents[i] === "ResourceType") {
                        value = resource.shortName;
                        compName = resource.resource;
                        componentsListItem.innerHTML = exampleComponents[i] + " " + note + ": " + compName + " (" + value + ")";
                    } else if (exampleComponents[i] === "Org") {
                        value = components[exampleComponents[i]].Short;
                        compName = components[exampleComponents[i]].Name;
                        componentsListItem.innerHTML = exampleComponents[i] + " " + note + ": " + compName + " (" + value + ")";
                    } else {
                        value = components[exampleComponents[i]];
                        componentsListItem.innerHTML = exampleComponents[i] + " " + note + ": " + value;
                    }
                    data.push(value);
                    componentsList.appendChild(componentsListItem);
                }

                name = data.join(delimiter);

                if (delimiter !== "") {
                    delimitValidation = validateDelimiter(name, resource, delimiter);
                    if (delimitValidation) {
                        name = data.join('');
                    }
                }
            }

            if (name) {
                // Appends Name to P tag
                let resourceName = document.createElement("p");
                resourceName.innerHTML = name;
                resourceName.className = "example";
                output.appendChild(resourceName);

                // Append Components Breakdown List
                if (resource.staticValues == "") {
                    let componentsListHeader = document.createElement("p");
                    componentsListHeader.innerHTML = "<strong>Components:</strong>";
                    output.appendChild(componentsListHeader);
                    output.appendChild(componentsList);
                }

                // Appends Static Value message to HTML elements
                if (resource.staticValues) {
                    let staticHeader = document.createElement("p");
                    staticHeader.className = "warning";
                    staticHeader.innerHTML = "<strong>Static Value Warning:</strong>";
                    output.appendChild(staticHeader);
                    let staticMessage = document.createElement("ul");
                    staticMessage.className = "warning";
                    staticMessage.innerHTML = "<li>This resource has unique naming requirements and cannot be generated using naming components.</li>";
                    output.appendChild(staticMessage);
                }

                // Appends Delimiter Warning to HTML elements
                if (delimitValidation && resource.staticValues == "") {
                    let delimitHeader = document.createElement("p");
                    delimitHeader.className = "warning";
                    delimitHeader.innerHTML = "<strong>Delimiter Warning:</strong>";
                    output.appendChild(delimitHeader);
                    let delimitMessage = document.createElement("ul");
                    delimitMessage.className = "warning";
                    delimitMessage.innerHTML = "<li>The delimiter is an invalid character for this resource and has been removed.</li>";
                    output.appendChild(delimitMessage);
                }

                // Appends Validation Warnings to HTML elements
                let validation = validateName(name, resource);
                if (validation.innerHTML !== "" && resource.staticValues == "") {
                    let validHeader = document.createElement("p");
                    validHeader.className = "warning";
                    validHeader.innerHTML = "<strong>Validation Warnings:</strong>";
                    output.appendChild(validHeader);
                    output.appendChild(validation);
                }
            }

            return output;
        }

        // Create Name based on form inputs in the Generator div
        function generateName() {
            let form = document.getElementsByClassName("data");
            let resourceName = document.getElementById("rsrc").value.split(", ");
            let resource = new Object;
            if (resourceName[1]) {
                resource = components.ResourceType.filter(function (resource, i) {
                    return (resource["resource"] == resourceName[0] && resource["property"] == resourceName[1]);
                })[0]
            } else {
                resource = components.ResourceType.find(({ resource }) => resource === resourceName[0]);
            }
            let data = new Array();
            for (let i = 0; i < form.length; i++) {
                if (form[i].value !== "") {
                    data.push(form[i].value);
                }
            }

            let name = data.join(delimiter);
            let validationHtml = "";
            if (delimiter !== "") {
                let delimitValidation = validateDelimiter(name, resource, delimiter);
                if (delimitValidation) {
                    name = data.join('');
                    validationHtml = "<p><strong>Delimiter Warning:</strong></p><ul><li>The delimiter is an invalid character for this resource and has been removed.</li></ul>";
                }
            }
            displayName.innerHTML = '<p><strong>Name:</strong> <span id="name">' + name + '</span></p>';
            let validation = validateName(name, resource);
            if (validation.innerHTML !== "") {
                validationHtml = validationHtml + "<p><strong>Validation Warnings:</strong></p>";
            }
            displayValidation.innerHTML = validationHtml;
            displayValidation.appendChild(validation);
        }

        function copyName() {
            /* Get the text field */
            let text = document.getElementById("name").textContent;
            navigator.clipboard.writeText(text)
                .then(() => {
                    console.log('Text copied to clipboard');
                })
                .catch(err => {
                    console.error('Error in copying text: ', err);
                });
        }

        // Search for a Resource based on the form input on the Reference div
        function resourceSearch() {
            let input, filter, table, tr, td, i, txtValue;
            input = document.getElementById('search');
            filter = input.value.toUpperCase();
            table = document.getElementById("resources");
            tr = table.getElementsByTagName('tr');


            // Loop through all table rows, and hide those who don't match the search query
            for (i = 0; i < tr.length; i++) {
                td = tr[i].getElementsByTagName("td")[0];
                if (td) {
                    txtValue = td.textContent || td.innerText;
                    if (txtValue.toUpperCase().indexOf(filter) > -1) {
                        tr[i].style.display = "";
                    } else {
                        tr[i].style.display = "none";
                    }
                }
            }
        }

        // Navigation function:
        // updates the class on the nav link to "current" when clicked &
        // hides and shows Divs to mimic separate webpages
        function displayDiv(id) {
            let children = document.getElementById("nav").getElementsByTagName("a");
            for (let i = 0; i < children.length; i++) {
                let child = children[i].id;
                if (child.includes(id)) {
                    document.getElementById(child).className = "current";
                    document.getElementById(id).style.display = "block";
                } else {
                    document.getElementById(child).className = "";
                    document.getElementById(child.replace("Link", "")).style.display = "none";
                }
            }
        }

        // Create Components table on Configuration div
        let config = Object.keys(components);
        let tableConfig = document.getElementById("compConfig");
        for (let i = 0; i < config.length; i++) {
            let trConfig = document.createElement("tr");

            // Column 1 in table
            let td1Config = document.createElement("td");
            td1Config.innerHTML = i + 1;
            trConfig.appendChild(td1Config);

            // Column 2 in table
            let td2Config = document.createElement("td");
            td2Config.innerHTML = config[i];
            trConfig.appendChild(td2Config);
            tableConfig.appendChild(trConfig);
        }

        // Create Config tables on Configuration div
        function createConfigTable(component) {
            let items = components[component];
            let table = document.getElementById(component);
            for (let i = 0; i < items.length; i++) {
                let tr = document.createElement("tr");

                // Column 1 in table
                let td1 = document.createElement("td");
                td1.innerHTML = items[i].Name;
                tr.appendChild(td1);

                // Column 2 in table
                let td2 = document.createElement("td");
                td2.innerHTML = items[i].Short;
                tr.appendChild(td2);

                // Add table row to table
                table.appendChild(tr);
            }
        }

        // If component was selected, add data to table on Configuration div
        let keys = Object.keys(components);
        if (keys.indexOf("Environment") >= 0) {createConfigTable("Environment");}
        if (keys.indexOf("Location") >= 0) {createConfigTable("Location");}
        if (keys.indexOf("UnitDept") >= 0) {createConfigTable("UnitDept");}
        if (keys.indexOf("ProjAppSvc") >= 0) {createConfigTable("ProjAppSvc");}
        if (keys.indexOf("VmRole") >= 0) {createConfigTable("VmRole");}

        // Create Resources table on Reference div
        let resources = components.ResourceType
        let table = document.getElementById("resources")
        for (let i = 0; i < resources.length; i++) {
            let tr = document.createElement("tr");

            // Column 1 in table
            let td1 = document.createElement("td");
            td1.className = "type";
            if (resources[i].property) {
                td1.innerHTML = resources[i].resource + ", " + resources[i].property;
            } else {
                td1.innerHTML = resources[i].resource;
            }
            tr.appendChild(td1);

            // Column 2 in table
            let td2 = document.createElement("td");
            td2.appendChild(generateExample(resources[i]));
            tr.appendChild(td2);

            // Column 3 in table
            let td3 = document.createElement("td");
            let shortName = document.createElement("p");
            shortName.innerHTML = "<strong>Short Name:</strong> " + resources[i].shortName;
            td3.appendChild(shortName);
            let scope = document.createElement("p");
            scope.innerHTML = "<strong>Scope:</strong> " + resources[i].scope;
            td3.appendChild(scope);
            let length = document.createElement("p");
            length.innerHTML = "<strong>Length:</strong> " + resources[i].lengthMin + " - " + resources[i].lengthMax + " characters";
            td3.appendChild(length);
            let valid = document.createElement("p");
            valid.innerHTML = "<strong>Valid Characters:</strong> " + resources[i].validText;
            td3.appendChild(valid);
            let invalid = document.createElement("p");
            invalid.innerHTML = "<strong>Invalid Characters:</strong> " + resources[i].invalidText;
            td3.appendChild(invalid);
            tr.appendChild(td3);
            table.appendChild(tr);
        }

        // Populates the Azure Resources drop down menu on the Generator div
        let select = document.getElementById("rsrc");
        let options = new Array();
        for (let i = 0; i < resources.length; i++) {
            if (resources[i].staticValues === "" && scopes.indexOf(resources[i].scope) >= 0) {
                let optionGn = document.createElement("option");
                if (components.ResourceType[i].property) {
                    optionGn.text = resources[i].resource + ", " + resources[i].property;
                    optionGn.value = resources[i].resource + ", " + resources[i].property;
                } else {
                    optionGn.text = resources[i].resource;
                    optionGn.value = resources[i].resource;
                }
                select.add(optionGn);
            }
        }
    </script>
</body>

</html>
'@

$Html = $Html -replace "{organization}",$Output4.Name

$Html = $Html -replace "{delimiter}",$output3

$Html = $Html -replace "{components}",$ObjectJson

$Html | Out-File -FilePath ".\AzureNamingTool.html" -Force
Write-Host "Your Naming Tool (AzureNamingTool.html) has been saved to your current console location."
Write-Host ""
