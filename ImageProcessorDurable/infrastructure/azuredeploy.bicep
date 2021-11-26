
targetScope = 'resourceGroup'

module infra '../../modules/infrastructure.bicep' = {
  name: 'chaindemo-deploy'
  params: {
    baseName: 'serverlessdemo'
    location: 'eastus2'
    containers: [
      'raw'
      'original'
      'resized'
      'image-data'
    ]
  }
}

module infraDurable '../../modules/infrastructure.bicep' = {
  name: 'durabledemo-deploy'
  params: {
    baseName: 'serverlessdemo'
    location: 'eastus2'
    containers: [
      'raw'
      'original'
      'resized'
    ]
    functionAppVersion: '~6'
  }
}
