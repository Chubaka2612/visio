variable "location" {
  type        = string
  description = "The Azure region where all resources will be deployed, e.g., 'East US'."
}

variable "application_name" {
  type        = string
  description = "The base name of the application. Used to construct resource names."
}

variable "environment" {
  type        = string
  description = "The environment name for deployment, e.g., 'dev', 'qa', 'prod'."
}

variable "resource_group_name" {
  type        = string
  description = "The name of the Azure Resource Group where the web app and associated resources will be deployed."
}

variable "cosmos_connection_string" {
  description = "The Cosmos DB connection string"
  type        = string
}

variable "cosmos_db_name" {
  description = "The Cosmos DB database name"
  type        = string
}

variable "blob_connection_string" {
  description = "The Blob Storage connection string"
  type        = string
}

variable "blob_container_name" {
  description = "The name of the Blob Storage container"
  type        = string
}

variable "blob_container_sas" {
  description = "The shared access signature of the storage account blob container"
  type        = string
}

variable "servicebus_connection_string" {
  description = "The Service Bus connection string"
  type        = string
}

variable "servicebus_topic_name" {
  description = "The name of the Service Bus topic"
  type        = string
}

variable "servicebus_topic_id" {
  description = "The ID of the service bus topic"
  type        = string
}

variable "cognitive_account_endpoint" {
  description = "The endpoint of the cognitive account"
  type        = string
}

variable "cognitive_account_primary_access_key" {
  description = "The primary access key of the cognitive account"
  type        = string
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "(Optional) A mapping of tags which should be assigned to the resources."
}