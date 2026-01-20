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

resource storageBlobService 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    isVersioningEnabled: true
  }
}

resource storageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: 'documents'
  parent: storageBlobService
  properties: {
    publicAccess: 'None'
  }
}

output storageName string = storageAccount.name
