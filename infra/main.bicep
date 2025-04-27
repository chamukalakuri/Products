@description('Specifies the location for resources.')
param location2 string = 'northeurope'

@description('Specifies the location for resources.')
param location string = 'northeurope'

@description('Tags that our resources need')
param tags object

@description('The name of the app service plan')
param appServicePlanName string

@description('The name of the webapp app')
param webAppName string

@description('The name of the storage account')
param storageAccountName string

@description('The name for the SQL API database')
param sqlDatabaseName string = 'orders'

@description('The name of the frontDoor Name')
param frontDoorName string

@description('Sql Server DB account name')
param sqlServerName string 

@description('Application Insights resource name')
param applicationInsightsName string

@description('Log Analytics resource name')
param logAnalyticsWorkspaceName string 

module compute 'compute.bicep'= {
  name: 'compute-deployment'
  params: {
    location: location
    tags: tags
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    storageAccountName: storageAccountName
    applicationInsightsName: applicationInsightsName
  }
  dependsOn:[
    storage
  ]
}

module storage 'storage.bicep'= {
  name: 'storage-deployment'
  params: {
    location: location2
    sqlServerName: sqlServerName
    databaseName:sqlDatabaseName
    frontDoorName: frontDoorName
    tags: tags
    applicationInsightsName: applicationInsightsName
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
  }
}
