param project_name string
param project_suffix string
param environment_name string
param location string
param vaults_kv_name string
param key_vault_key_values array

@description('Key Vault')
resource vaults_kv_resource 'Microsoft.KeyVault/vaults@2024-12-01-preview' = {
  name: vaults_kv_name
  location: location
  tags: union(resourceGroup().tags, {
    //     some_extra_tag: 'some_value'
  })
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: '76960b76-9480-40b3-bb44-6db1fefb433d'
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
    accessPolicies: []
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: true
    vaultUri: 'https://${vaults_kv_name}.vault.azure.net/'
    provisioningState: 'Succeeded'
    publicNetworkAccess: 'Enabled'
  }
}

@description('Key Vault Secrets')
// ACT: Once created add or run script to add the correct secret values to these keys
resource kv_secret_resource 'Microsoft.KeyVault/vaults/secrets@2024-12-01-preview' = [
  for kv in key_vault_key_values: {
    parent: vaults_kv_resource
    name: kv.name
    properties: {
      value: kv.value
      attributes: {
        enabled: true
      }
    }
  }
]

@description('Managed Identity for Key Vault access')
// ACT: Managed Identity need "Key Vault Secrets User" role assignment (in the Key Vault) to read secrets
var managed_identity_name string = 'mi-${project_name}-${environment_name}-${project_suffix}-kv'
resource managed_user_assigned_identity_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' = {
  name: managed_identity_name
  location: location
}
