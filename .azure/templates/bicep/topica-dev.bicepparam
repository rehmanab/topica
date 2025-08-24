// az deployment group create --name TopicaStaging --resource-group rg-topica-staging-001 --parameters topica-dev.bicepparam --debug

using './topica.all.bicep'

@allowed(['dev', 'staging', 'prod'])
@description('Environment name')
param environment_Name = 'staging'

@description('Name of the Key Vault for this Environment')
var vaults_kv_prefix = 'kv'
var vaults_kv_project_name = 'topica'
var vaults_kv_number = '003'
param vaults_kv_name = toLower('${vaults_kv_prefix}-${vaults_kv_project_name}-${environment_Name}-${vaults_kv_number}')