
targetScope = 'resourceGroup'
var suffix = substring(uniqueString(resourceGroup().name), 0, 5)
var location = 'eastus'

resource appPlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: 'plan-serverlessDemo-${suffix}'
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

var storageAccountName = 'stgserverlessdemo${suffix}'
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: length(storageAccountName) > 24 ? substring(storageAccountName, 0, 24) : storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource rawContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = {
  name: '${storageAccount.name}/default/raw'
  properties: {
    publicAccess: 'None'
  }
}

resource originalContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = {
  name: '${storageAccount.name}/default/original'
  properties: {
    publicAccess: 'None'
  }
}

resource resizedContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = {
  name: '${storageAccount.name}/default/resized'
  properties: {
    publicAccess: 'None'
  }
}

resource imageDataContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = {
  name: '${storageAccount.name}/default/image-data'
  properties: {
    publicAccess: 'None'
  }
}

resource cognitiveServices 'Microsoft.CognitiveServices/accounts@2021-04-30' = {
  name: 'cogserverlessdemo${suffix}'
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
  name: 'func-serverlessdemo-${suffix}'
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
          value: '~3'
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
