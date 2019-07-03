
**Learn more about Blueprints at The Azure Academy**
============================
	https://www.youtube.com/AzureAcademy
	
**Purpose of CAF Migration landing zone - governance blueprint**
============================
	This blueprint is one example of a [governance MVP](https://docs.microsoft.com/azure/architecture/cloud-adoption/governance/getting-started) that can be easily added to the migration landing zone to provide initial governance functions.
		
**What The Governance Blueprint will create for you:**
============================
	Azure Resource Groups
		  "SharedServices-RG": 
		  "Identity-RG": 
		  "Network-RG": 
		  "Application-RG": 
	Azure Policy
		Definitions
			Tagging (CostCenter)
				Tag Resource Group
				Append resources in the resource group with the CostCenter Tag
			Allowed Azure Region for Resources
			Allowed Storage Account SKUs
			Allowed Azure VM SKUs	
			Allowed Azure Resource Types
			Require Network Watch to be deployed 
			Require Azure Storage Account Secure transfer Encryption
		Initiatives
			Enable Monitoring in Azure Security Center (78 Policies)			
	Azure Templates
		Deploy Azure Key Vault 
		Deploy Azure Log Analytics Workspace
		Deploy Azure Security Center Standard
		Deploy Azure Virtual Network Hub

			
**Input Parameters:**
============================
	"Organization":				"Enter your organization name (e.g. Contoso), must be unique"
    "HUB-RG-Location":			"Select 1 Azure Region for Deployment"
    "Policy_Allowed-Locations":		"Which Azure Regions will you allow resources to be built in?"
    "Policy_Allowed-VM-SKUs":		"Allowed virtual machine SKUs"
    "Policy_Allowed-StorageAccount-SKUs":	"SKU used in Diagnostic Log storage accounts"
	"Policy_Allowed-Resource-Types":	"Which Azure Resources you want to allow in your environment "
    "Policy_CostCenter_Tag":		"Append CostCenter TAG & its value from the Resource Group"
    "LocalAdmin-Username":			"KeyVault-Secret LocalAdmin Username"
    "Local-Admin-Password":			"KeyVault-Secret LocalAdmin Password"
    "KeyVault-user-id":			"AAD object ID of the user that requires access to Key Vault."
	"LogAnalytics_DataRetention":		"Number of days data will be retained in Log Analytics"
    "LogAnalytics_Location":		"Region to use when establishing the workspace"


**END**
============================
