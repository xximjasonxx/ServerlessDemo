
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
