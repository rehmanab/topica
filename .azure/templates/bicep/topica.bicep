// Deploy the .bicepparam file, see command in comment there

// Scope to subscription level so resource group can be created
targetScope = 'subscription'

@description('Parameters are defined in the .bicepparam file')
// general parameters
param project_name string
param project_suffix string
param environment_name string
param location string

// resource group parameters
param rg_name string

// keyvault parameters
param vaults_kv_name string
param key_vault_key_values array

@description('Local parameter')
param createdDateTimeUtc string = utcNow()

@description('Resource Group')
resource newResourceGroup 'Microsoft.Resources/resourceGroups@2024-11-01' = {
  name: rg_name
  location: location
  tags: {
    project_name: project_name
    project_suffix: project_suffix
    environment_name: environment_name
    createdBy: 'Bicep Template'
    createdDateTimeUtc: createdDateTimeUtc
  }
}

module keyVault 'keyvault_module.bicep' = {
  name: 'keyvaultModule'
  scope: newResourceGroup
  params: {
    project_name: project_name
    project_suffix: project_suffix
    environment_name: environment_name
    location: location
    vaults_kv_name: vaults_kv_name
    key_vault_key_values: key_vault_key_values
  }
}
