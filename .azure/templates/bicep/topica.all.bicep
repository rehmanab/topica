// Deploy the .bicepparam file, see command in comment there

@description('Values are defined in the .bicepparam file')
param vaults_kv_name string
param environment_Name string
param location string = resourceGroup().location
param deploymentDateTime string = utcNow()

@description('Key Vault for Topica Staging Environment')
resource vaults_kv_resource 'Microsoft.KeyVault/vaults@2024-12-01-preview' = {
  name: vaults_kv_name
  location: location
  tags: union(resourceGroup().tags, {
    environment: environment_Name
    project: 'topica'
    createdBy: 'Bicep Template'
    deploymentDateTime: deploymentDateTime
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

resource vaults_kv_topica_staging_002_name_secret_name 'Microsoft.KeyVault/vaults/secrets@2024-12-01-preview' = {
  parent: vaults_kv_resource
  name: 'secret-name'
  properties: {
    value: 'XXX2'
    attributes: {
      enabled: true
    }
  }
}
