@description('The location')
param location string

@description('The postfix for the resources')
param resourcesPostfix string

var abbrs = loadJsonContent('./abbreviations.json')

var applicationInsightsName = toLower('${resourcesPostfix}-${abbrs.insightsComponents}')

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

output appInsightName string = applicationInsights.name
