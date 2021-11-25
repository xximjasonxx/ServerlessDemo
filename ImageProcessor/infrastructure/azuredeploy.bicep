
targetScope = 'resourceGroup'
var suffix = uniqueString(resourceGroup().name)

resource appPlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: 'plan-serverlessDemo'
  location: 'eastus2'
}
