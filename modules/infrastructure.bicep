
@minLength(5)
param baseName string

@allowed([
  'eastus'
  'eastus2'
  'westus'
])
param location string

@minLength(5)
param suffix string = substring(uniqueString(resourceGroup().name), 0, 5)

@minLength(1)
param containers array

param functionAppVersion string = '~3'

resource appPlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: 'plan-${baseName}-${suffix}'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
  properties: {
    reserved: true
  }
}

var storageAccountName = 'stg${baseName}${suffix}'
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: length(storageAccountName) > 24 ? substring(storageAccountName, 0, 24) : storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource saContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = [for container in containers: {
  name: '${storageAccount.name}/default/${container}'
  properties: {
    publicAccess: 'None'
  }
}]

resource cognitiveServices 'Microsoft.CognitiveServices/accounts@2021-04-30' = {
  name: 'cog${baseName}${suffix}'
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'CognitiveServices'
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: 'func-${baseName}-${suffix}'
  location: location
  kind: 'functionapp'
  properties: {
    httpsOnly: true
    serverFarmId: appPlan.id
    clientAffinityEnabled: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionAppVersion
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'CognitiveServicesEndpoint'
          value: '${cognitiveServices.properties.endpoint}'
        }
        {
          name: 'CognitiveServicesKey'
          value: listkeys(cognitiveServices.id, cognitiveServices.apiVersion).key1
        }
      ]
    }
  }
}
