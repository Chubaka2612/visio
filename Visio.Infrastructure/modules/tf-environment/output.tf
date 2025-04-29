output "resource_group" {
  description = "The object representing the resource group"
  value =  azurerm_resource_group.main
}

output "storage_account" {
  description = "The object representing the storage account"
  value = azurerm_storage_account.main
  sensitive = true
}

output "storage_container" {
  description = "The object representing the storage container"
  value =  azurerm_storage_container.images
}

output "storage_sas" {
  description = "The object representing the storage SAS token"
  value = data.azurerm_storage_account_blob_container_sas.sas
  sensitive = true
}

output "cosmosdb_account" {
  value       = azurerm_cosmosdb_account.main
  description = "The Cosmos DB account object."
}

output "cosmosdb_sql_database" {
  value       = azurerm_cosmosdb_sql_database.main
  description = "The Cosmos DB SQL database object."
}

output "cosmosdb_sql_container" {
  value       = azurerm_cosmosdb_sql_container.main
  description = "The Cosmos DB SQL container object."
}

output "servicebus_namespace" {
  value       = azurerm_servicebus_namespace.main
  description = "The Azure Service Bus namespace object."
}

output "servicebus_topic" {
  value       = azurerm_servicebus_topic.main
  description = "The Azure Service Bus topic object."
}

output "cognitive_account" {
  value       = azurerm_cognitive_account.vision
  description = "The Azure Cognitive Services Computer Vision account object."
}
