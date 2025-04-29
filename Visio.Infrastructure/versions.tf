terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  required_version = ">= 1.0"
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
  client_id       = "__CIAzureClientID__"
  client_secret   = "__CIAzureClientSecret__"
  tenant_id       = "__TenantID__"
  subscription_id = "__SubscriptionID__"
}