locals {
  name         = format("%s-%s", var.application_name, var.environment)
  tags         = merge(var.tags, {
    managed     = "terraform"
    project     = "visio"
    environment = var.environment
    responsible = "viktoriia_skirko@epam.com"
  })
}

resource "azurerm_service_plan" "web" {
  name                = format("%s-plan", local.name)
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Windows"
  sku_name            = "S1"
  tags                = var.tags
}

resource "azurerm_windows_web_app" "web" {
  name                = format("%s-web", local.name)
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.web.id

  site_config {
    always_on = false
    ftps_state = "Disabled"
    
    application_stack {
      dotnet_version = "v8.0"
    }
  }

  app_settings = {
    "AzureCosmosOptions:ConnectionString"         = var.cosmos_connection_string
    "AzureCosmosOptions:DatabaseId"               = var.cosmos_db_name

    "AzureBlobStorageOptions:ConnectionString"    = var.blob_connection_string
    "AzureBlobStorageOptions:ContainerId"         = var.blob_container_name

    "ServiceBusOptions:ConnectionString"          = var.servicebus_connection_string
    "ServiceBusOptions:QueueId"                   = var.servicebus_topic_name
  }

  tags = var.tags
}

resource "azurerm_service_plan" "function_plan" {
  name                = format("%s-function-plan", local.name)
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Windows"
  sku_name            = "Y1" # Consumption plan
  tags                = var.tags
}

resource "azurerm_servicebus_subscription" "main" {
  name                = format("sbs-%s", local.name)
  topic_id            = var.servicebus_topic_id
  max_delivery_count  = 10
}

resource "azurerm_storage_account" "function_storage" {
  name                     = format("st%sfunction", replace(local.name, "-", ""))
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_windows_function_app" "function_app" {
  name                = format("%s-function", local.name)
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.function_plan.id
  storage_account_name       = azurerm_storage_account.function_storage.name
  storage_account_access_key = azurerm_storage_account.function_storage.primary_access_key

  site_config {
    application_stack {
      dotnet_version = "v8.0"
    }
  }

  app_settings = {
    "FUNCTIONS_EXTENSION_VERSION"         = "~4"
    "WEBSITE_RUN_FROM_PACKAGE"            = "1"
    "FUNCTIONS_INPROC_NET8_ENABLED"       = "1"
    "FUNCTIONS_WORKER_RUNTIME"            = "dotnet"

    # Required storage account for function app
    "AzureWebJobsStorage"                = azurerm_storage_account.function_storage.primary_connection_string

    # Cosmos DB
    "AzureCosmosOptions.ConnectionString" = var.cosmos_connection_string
    "AzureCosmosOptions.DatabaseId"       = var.cosmos_db_name

    # Blob Storage
    "AzureBlobStorageOptions.SharedAccessToken" = var.blob_container_sas
    "AzureBlobStorageOptions.ContainerName"     = var.blob_container_name

    # Azure Cognitive Services
    "CompureVision.ApiKeyServiceClientCredentials" =  var.cognitive_account_primary_access_key
    "CompureVision.Endpoint"                       =  var.cognitive_account_endpoint

    # Service Bus
    "AzureServiceBusConnection" = var.servicebus_connection_string
    "ServiceBusTopic"           = var.servicebus_topic_name
    "ServiceBusSubscription"    = azurerm_servicebus_subscription.main.name
  }
}
