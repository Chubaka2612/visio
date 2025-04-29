locals {
  name         = format("%s-%s", var.application_name, var.environment)
  tags         = merge(var.tags, {
    managed     = "terraform"
    project     = "visio"
    environment = var.environment
    responsible = "viktoriia_skirko@epam.com"
  })
}

resource "azurerm_resource_group" "main" {
  name     = format("rg-%s-infra", local.name)
  location = var.location

  tags = local.tags
}

resource "azurerm_storage_account" "main" {
  name                     = format("st%s", replace(local.name, "-", ""))
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = local.tags
}

resource "azurerm_storage_container" "images" {
  name                  = "images"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

data "azurerm_storage_account_blob_container_sas" "sas" {
  connection_string = azurerm_storage_account.main.primary_connection_string
  container_name       = azurerm_storage_container.images.name
  https_only           = true

  start  = formatdate("YYYY-MM-DD", timestamp())                   # Current date
  expiry = formatdate("YYYY-MM-DD", timeadd(timestamp(), "8760h")) # 8760 hours = 1 year
  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    list   = false
    delete = false
  }
}

resource "azurerm_cosmosdb_account" "main" {
  name                = format("cosmos-%s", replace(local.name, "-", ""))
  location            = "eastus2" #TODO: to avoid 'Sorry, we are currently experiencing high demand in East US region'
  resource_group_name = azurerm_resource_group.main.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = "eastus2"#TODO
    failover_priority = 0
  }

  tags = local.tags
}

resource "azurerm_cosmosdb_sql_database" "main" {
  name                = "imagesdb"
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
  throughput          = 400
}

resource "azurerm_cosmosdb_sql_container" "main" {
  name                  = "images"
  resource_group_name   = azurerm_resource_group.main.name
  account_name          = azurerm_cosmosdb_account.main.name
  database_name         = azurerm_cosmosdb_sql_database.main.name
  partition_key_path    = "/id"
  throughput            = 400
}

resource "azurerm_servicebus_namespace" "main" {
  name                = format("sb-%s", local.name)
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Standard"

  tags = local.tags
}

resource "azurerm_servicebus_topic" "main" {
  name                = format("sbt-%s", local.name)
  namespace_id        = azurerm_servicebus_namespace.main.id
  enable_partitioning = true
}

resource "azurerm_cognitive_account" "vision" {
  name                = format("cog-%s-vision", local.name)
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  kind                = "ComputerVision"
  sku_name            = "S1"

  tags = local.tags
}
