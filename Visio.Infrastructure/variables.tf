variable "location" {
  type        = string
  default     = "eastus"
  description = "(Optional) The Azure Region where the Resources should exist. Changing this forces a new Resource Group to be created."
}

variable "environment" {
  type        = string
  default     = "eastus"
  description = "(Required) The short environment name, for example dev, test, uat, prod."
}

variable "application_name" {
  type        = string
  default     = "visio"
  description = "(Optional) The short name of an application this infrastructure is used to host. Used for resource naming."
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "(Optional) A mapping of tags which should be assigned to the resources."
}