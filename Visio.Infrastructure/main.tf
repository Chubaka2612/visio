module "tf-environment" {
  source = "./modules/tf-environment"

  environment        = var.environment
  location           = var.location
  application_name   = var.application_name
}

module "tf-application" {
  source             = "./modules/tf-application"
  environment        = var.environment
  location           = var.location
  application_name   = var.application_name

  resource_group_name          = module.tf-environment.resource_group.name
 
  cosmos_connection_string     = module.tf-environment.cosmosdb_account.connection_strings[0]
  cosmos_db_name               = module.tf-environment.cosmosdb_sql_database.name
 
  blob_container_sas           = module.tf-environment.storage_sas.sas
  blob_connection_string       = module.tf-environment.storage_account.primary_connection_string
  blob_container_name          = module.tf-environment.storage_container.name
 
  servicebus_connection_string = module.tf-environment.servicebus_namespace.default_primary_connection_string
  servicebus_topic_name        = module.tf-environment.servicebus_topic.name
  servicebus_topic_id          = module.tf-environment.servicebus_topic.id

  cognitive_account_endpoint           = module.tf-environment.cognitive_account.endpoint
  cognitive_account_primary_access_key = module.tf-environment.cognitive_account.primary_access_key
}