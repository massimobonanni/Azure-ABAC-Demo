@description('The location')
param location string

@description('The postfix for the resources')
param resourcesPostfix string

@description('The name of the application insight tied to the front end')
param applicationInsightName string

@description('The name of the storage account used by the front end')
param storageAccountName string

var abbrs = loadJsonContent('./abbreviations.json')

var appServiceName = toLower('${resourcesPostfix}-${abbrs.webSitesAppService}')
var appServicePlanName = toLower('${resourcesPostfix}-${abbrs.webServerFarms}')

resource applicationInsight 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' existing = {
  name: storageAccountName
}

resource frontEndAppServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: false
  }
  sku: {
    name:'F1'
  }
  kind: 'windows'
}

resource frontEndAppService 'Microsoft.Web/sites@2021-02-01' = {
  name: appServiceName
  location: location
  kind: 'app'
  tags: {
    'azd-service-name': 'abacdemo-web'
  }
  properties: {
    httpsOnly: true
    serverFarmId: frontEndAppServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v10.0'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource basicPublishingCredentials 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: frontEndAppService
  name: 'scm'
  properties: {
    allow: true
  }
}

resource appSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'appsettings'
  parent: frontEndAppService
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsight.properties.InstrumentationKey
    APPINSIGHTS_CONNECTION_STRING: 'InstrumentationKey=${applicationInsight.properties.InstrumentationKey}'
    StorageAccountBlobUri: storageAccount.properties.primaryEndpoints.blob
  }
}

