@description('The location')
param location string

@description('The postfix for the resources')
param resourcesPostfix string

var abbrs = loadJsonContent('./abbreviations.json')

var storageName  = toLower('${resourcesPostfix}${abbrs.storageStorageAccounts}')

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
} 

output storageName string = storageAccount.name
