@description('Location for the resources')
param location string = resourceGroup().location

@description('Tags that our resources need')
param tags object

@description('The name of the web app')
param webAppName string

@description('The name of the storage account')
param storageAccountName string

@description('The name of the app service plan')
param appServicePlanName string

@description('Application Insights resource name')
param applicationInsightsName string 

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_ZRS'
  }
  kind: 'StorageV2'
  identity: {
    type: 'None'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    networkAcls: {
        bypass: 'AzureServices'
        defaultAction: 'Allow'
    }
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
  kind: 'elastic'
}

// Web App
resource webApp 'Microsoft.Web/sites@2021-03-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: netFrameworkVersion
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~2'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'default'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
          type: 'SQLAzure'
        }
      ]
      cors: {
        allowedOrigins: [
          'https://${frontDoor.properties.frontendEndpoints[0].properties.hostName}'
        ]
      }
      healthCheckPath: '/health'
      // Enable always-on to keep the app warm
      alwaysOn: true
      // Configure minimal TLS version for security
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

resource authsettings 'Microsoft.Web/sites/config@2022-09-01' = {
  name: 'authsettingsV2'
  kind: 'string'
  parent: webApp  // Reference to your Web App resource
  properties: {
    // Same properties as in your example
  }
}

//Outputs
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
