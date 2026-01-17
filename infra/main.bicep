targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string='ABACDemo'

@description('The location of the resources')
param location string = deployment().location

@description('The prefix for the resources')
param resourcesPostfix string = 'ABAC${uniqueString(subscription().id,environmentName)}'

var abbrs = loadJsonContent('./abbreviations.json')

var resourceGroupName = '${environmentName}-${abbrs.resourcesResourceGroups}'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: location
}

module appInsight 'applicationInsight.bicep' = {
  scope: resourceGroup
  name: 'appInsight'
  params: {
    location: location
    resourcesPostfix: resourcesPostfix
  }
}

module frontEnd 'frontEnd.bicep' = {
  scope: resourceGroup  
  name: 'frontEnd'
  params: {
    location: location
    resourcesPostfix: resourcesPostfix
    applicationInsightName: appInsight.outputs.appInsightName
    storageAccountName:storage.outputs.storageName
  }
}

module storage 'storage.bicep' = {
  scope: resourceGroup
  name: 'storage'
  params: {
    location: location
    resourcesPostfix: resourcesPostfix
  }
}
