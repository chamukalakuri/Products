@description('sqlServer DB account name')
param sqlServerName string 

@description('Location for the DB account.')
param location string = resourceGroup().location

@description('The name for the SQL API database')
param sqlDatabaseName string

@description('Tags that our resources need')
param tags object

@description('Application Insights resource name')
param applicationInsightsName string


@description('Front Door resource name')
param frontDoorName string

@description('Log Analytics resource name')
param logAnalyticsWorkspaceName string 


// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }

  // SQL Server Firewall Rules
  resource firewallRule 'firewallRules@2021-08-01-preview' = {
    name: 'AllowAllAzureIPs'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0' // Allow Azure services
    }
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 1073741824 // 1GB
  }
}

// Front Door
resource frontDoor 'Microsoft.Network/frontDoors@2021-06-01' = {
  name: frontDoorName
  location: 'global'
  properties: {
    enabledState: 'Enabled'
    
    // Front Door routing settings
    routingRules: [
      {
        name: 'APIRoutingRule'
        properties: {
          frontendEndpoints: [
            {
              id: resourceId('Microsoft.Network/frontDoors/frontendEndpoints', frontDoorName, 'APIEndpoint')
            }
          ]
          acceptedProtocols: [
            'Https'
          ]
          patternsToMatch: [
            '/*'
          ]
          routeConfiguration: {
            '@odata.type': '#Microsoft.Azure.FrontDoor.Models.FrontdoorForwardingConfiguration'
            forwardingProtocol: 'HttpsOnly'
            backendPool: {
              id: resourceId('Microsoft.Network/frontDoors/backendPools', frontDoorName, 'APIBackendPool')
            }
          }
          enabledState: 'Enabled'
        }
      }
    ]
    
    // Health probe settings
    healthProbeSettings: [
      {
        name: 'HealthProbeSettings'
        properties: {
          path: '/health'
          protocol: 'Https'
          intervalInSeconds: 30
          healthProbeMethod: 'GET'
          enabledState: 'Enabled'
        }
      }
    ]
    
    // Load balancing settings
    loadBalancingSettings: [
      {
        name: 'LoadBalancingSettings'
        properties: {
          sampleSize: 4
          successfulSamplesRequired: 2
          additionalLatencyMilliseconds: 0
        }
      }
    ]
    
    // Backend pools
    backendPools: [
      {
        name: 'APIBackendPool'
        properties: {
          backends: [
            {
              address: webApp.properties.defaultHostName
              httpPort: 80
              httpsPort: 443
              weight: 100
              priority: 1
              enabledState: 'Enabled'
            }
          ]
          loadBalancingSettings: {
            id: resourceId('Microsoft.Network/frontDoors/loadBalancingSettings', frontDoorName, 'LoadBalancingSettings')
          }
          healthProbeSettings: {
            id: resourceId('Microsoft.Network/frontDoors/healthProbeSettings', frontDoorName, 'HealthProbeSettings')
          }
        }
      }
    ]
    
    // Frontend endpoints
    frontendEndpoints: [
      {
        name: 'APIEndpoint'
        properties: {
          hostName: '${frontDoorName}.azurefd.net'
          sessionAffinityEnabledState: 'Disabled'
          sessionAffinityTtlSeconds: 0
          webApplicationFirewallPolicyLink: {
            id: wafPolicy.id
          }
        }
      }
    ]
  }
}

// Web Application Firewall Policy for Front Door
resource wafPolicy 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2020-11-01' = {
  name: '${frontDoorName}-waf-policy'
  location: 'global'
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: 'Prevention'
      customBlockResponseStatusCode: 403
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'DefaultRuleSet'
          ruleSetVersion: '1.0'
        }
        {
          ruleSetType: 'Microsoft_BotManagerRuleSet'
          ruleSetVersion: '1.0'
        }
      ]
    }
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}



// Outputs
output frontDoorUrl string = 'https://${frontDoor.properties.frontendEndpoints[0].properties.hostName}'
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDatabase.name
