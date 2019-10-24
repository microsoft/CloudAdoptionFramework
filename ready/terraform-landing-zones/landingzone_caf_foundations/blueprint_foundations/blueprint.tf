
#Create the resource groups to host the blueprint
module "resource_group_hub" {
  source  = "aztfmod/caf-resource-group/azurerm"
  version = "0.1.1"
  
  prefix          = var.prefix
  resource_groups = var.resource_groups_hub
  tags            = var.tags_hub
}

#Specify the subscription logging repositories 
module "activity_logs" {
  source  = "aztfmod/caf-activity-logs/azurerm"
  version = "0.1.1"

  prefix              = var.prefix
  resource_group_name = module.resource_group_hub.names["HUB-CORE-SEC"]
  location            = var.location_map["region1"]
  tags                = var.tags_hub
  logs_rentention     = var.azure_activity_logs_retention
}

#Specify the operations diagnostic logging repositories 
module "diagnostics_logging" {
  source  = "aztfmod/caf-diagnostics-logging/azurerm"
  version = "0.1.1"

  prefix                = var.prefix
  resource_group_name   = module.resource_group_hub.names["HUB-OPERATIONS"]
  location              = var.location_map["region1"]
  tags                  = var.tags_hub
}

#Create the Azure Monitor - Log Analytics workspace
module "log_analytics" {
  source  = "aztfmod/caf-log-analytics/azurerm"
  version = "0.1"

  prefix              = var.prefix
  name                = var.analytics_workspace_name
  resource_group_name = module.resource_group_hub.names["HUB-OPERATIONS"]
  location            = var.location_map["region1"]
  tags                = var.tags_hub
  solution_plan_map   = var.solution_plan_map
}

Create the Azure Security Center workspace
module "security_center" {
  source  = "aztfmod/caf-security-center/azurerm"
  version = "0.1"

  contact_email = var.security_center["contact_email"]
  contact_phone = var.security_center["contact_phone"]
  scope_id      = "/subscriptions/${data.azurerm_client_config.current.subscription_id}"
  workspace_id  = module.log_analytics.id
}

