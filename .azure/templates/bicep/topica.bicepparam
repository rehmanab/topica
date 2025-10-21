// Deploy using rg scope
// az deployment group create --name TopicaDeploy1 --resource-group rg-topica-staging-001 --parameters topica.bicepparam --debug

// Deploy using subscription scope
// az deployment sub create --name TopicaDeploy1 --location uksouth --parameters topica.bicepparam --debug
  
using './topica.bicep'

@allowed(['dev', 'staging', 'prod'])
@description('Variables defined for this Environment')
param project_name = 'topica'
param project_suffix  = '001'
param environment_name = 'dev'
param location = 'uksouth'

param key_vault_key_values = [
  {
    name: 'AwsHostSettings--AccessKey'
    value: 'XXX'
  }
  {
    name: 'AwsHostSettings--ProfileName'
    value: ''
  }
  {
    name: 'AwsHostSettings--RegionEndpoint'
    value: 'XXX'
  }
  {
    name: 'AwsHostSettings--SecretKey'
    value: 'XXX'
  }
  {
    name: 'AwsHostSettings--ServiceUrl'
    value: ''
  }
]

@description('Resource Group Name')
param rg_name = toLower('rg-${project_name}-${environment_name}-${project_suffix}')

@description('Key Vault Name')
param vaults_kv_name = toLower('kv-${project_name}-${environment_name}-${project_suffix}')
