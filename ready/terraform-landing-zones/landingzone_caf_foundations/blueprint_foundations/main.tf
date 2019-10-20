
data "azurerm_client_config" "current" {
}

provider "azurerm" {
  version = "<= 1.35.0"
}

provider "azuread" {
    version = "<=0.6.0"
}

terraform {
    backend "azurerm" {
    }
}

