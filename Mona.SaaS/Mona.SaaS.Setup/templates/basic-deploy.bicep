@description('A unique, publisher-defined name that identifies this Mona deployment.')
@maxLength(13)
param deploymentName string = ''

@description('This Mona deployment\'s Azure Active Directory (AAD) tenant ID.')
param aadTenantId string

@description('This Mona deployment\'s Azure Active Directory (AAD) client ID.')
param aadClientId string

@description('This Mona deployment\'s Azure Active Directory (AAD) enterprise application/principal object ID.')
param aadPrincipalId string

@description('This Mona deployment\'s Azure Active Directory (AAD) client secret.')
@secure()
param aadClientSecret string

@description('If provided, specifies the ID of an existing app service plan that the Mona web app should be deployed to.')
param appServicePlanId string = ''

@description('This Mona deployment\'s Marketplace client Azure Active Directory (AAD) client ID.')
param aadMpClientId string

@description('This Mona deployment\'s Marketplace client Azure Active Directory (AAD) client secret.')
@secure()
param aadMpClientSecret string

@description('Flag indicates passthroughMode is enabled by default')
param isPassthroughModeEnabled bool = false

@description('The version of events that this Mona deployment will publish to Event Grid.')
@allowed([
  '2021-05-01'
  '2021-10-01'
])
param eventVersion string = '2021-10-01'

@description('The preferred UI language for this Mona deployment.')
@allowed([
  'en'
  'es'
])
param language string = 'en'
param location string = resourceGroup().location

var monaVersion = '0.1-prerelease'
var cleanDeploymentName = toLower((empty(deploymentName) ? uniqueString(resourceGroup().id) : deploymentName))
var deploymentNameUnique = uniqueString(resourceGroup().id, deployment().name, cleanDeploymentName)
var storageAccountName = 'monastorage${deploymentNameUnique}'
var blobServiceName = 'default'
var configContainerName = 'configuration'
var testSubContainerName = 'test-subscriptions'
var stageSubContainerName = 'stage-subscriptions'
var eventGridTopicName = 'mona-events-${cleanDeploymentName}'
var logAnalyticsName = 'mona-logs-${cleanDeploymentName}'
var appInsightsName = 'mona-app-insights-${cleanDeploymentName}'
var appPlanName = 'mona-plan-${cleanDeploymentName}'
var webAppName = 'mona-web-${cleanDeploymentName}'
var logicApps_ui = {
  en: {
    eventGridConnectionDisplayName: 'Mona Subscription Events'
  }
}
var logicApps = {
  eventGridConnectionName: 'mona-events-connection-${cleanDeploymentName}'
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'Standalone'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: -1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: 90
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource storageAccountName_default 'Microsoft.Storage/storageAccounts/managementPolicies@2019-06-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    policy: {
      rules: [
        {
          name: 'AgeOffStagedSubscriptions'
          enabled: true
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                stageSubContainerName
              ]
            }
            actions: {
              baseBlob: {
                delete: {
                  daysAfterModificationGreaterThan: 30
                }
              }
            }
          }
        }
        {
          name: 'AgeOffTestSubscriptions'
          enabled: true
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                testSubContainerName
              ]
            }
            actions: {
              baseBlob: {
                delete: {
                  daysAfterModificationGreaterThan: 30
                }
              }
            }
          }
        }
      ]
    }
  }
}

resource storageAccountName_blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-01-01' = {
  parent: storageAccount
  name: blobServiceName
  properties: {
    isVersioningEnabled: true
  }
}

resource storageAccountName_blobServiceName_testSubContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-01-01' = {
  parent: storageAccountName_blobService
  name: testSubContainerName
  properties: {
    publicAccess: 'None'
  }
  
}

resource storageAccountName_blobServiceName_stageSubContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-01-01' = {
  parent: storageAccountName_blobService
  name: stageSubContainerName
  properties: {
    publicAccess: 'None'
  }
  
}

resource storageAccountName_blobServiceName_configContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-01-01' = {
  parent: storageAccountName_blobService
  name: configContainerName
  properties: {
    publicAccess: 'None'
  }
  
}

resource eventGridTopic 'Microsoft.EventGrid/topics@2020-06-01' = {
  name: eventGridTopicName
  location: location
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
  }
}

resource appPlan 'Microsoft.Web/serverfarms@2018-02-01' = if (empty(appServicePlanId)) {
  name: appPlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
    size: 'S1'
    family: 'S'
    capacity: 1
  }
  properties: {
  }
  kind: 'app'
}

resource webApp 'Microsoft.Web/sites@2018-11-01' = {
  name: webAppName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: (empty(appServicePlanId) ? appPlan.id : appServicePlanId)
  }
  dependsOn: [

    appInsights
  ]
}

resource webAppName_appsettings 'Microsoft.Web/sites/config@2020-12-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    'Deployment:AppInsightsInstrumentationKey': reference(appInsights.id, '2014-04-01').InstrumentationKey
    'Deployment:AzureResourceGroupName': resourceGroup().name
    'Deployment:AzureSubscriptionId': subscription().subscriptionId
    'Deployment:EventVersion': eventVersion
    'Deployment:IsTestModeEnabled': 'true'
    'Deployment:MonaVersion': monaVersion
    'Deployment:Name': cleanDeploymentName
    'Deployment:IsPassthroughModeEnabled' : string(isPassthroughModeEnabled)
    'Identity:AdminIdentity:AadTenantId': aadTenantId
    'Identity:AdminIdentity:RoleName': 'monaadmins'
    'Identity:AppIdentity:AadClientId': aadClientId
    'Identity:AppIdentity:AadClientSecret': aadClientSecret
    'Identity:AppIdentity:AadPrincipalId': aadPrincipalId
    'Identity:AppIdentity:AadTenantId': aadTenantId
    'Identity:MarketplaceIdentity:AadClientId': aadMpClientId
    'Identity:MarketplaceIdentity:AadClientSecret': aadMpClientSecret
    'Identity:MarketplaceIdentity:AadTenantId': aadTenantId
    'Subscriptions:Events:EventGrid:TopicEndpoint': eventGridTopic.properties.endpoint
    'Subscriptions:Events:EventGrid:TopicKey': listKeys(eventGridTopic.id, '2020-04-01-preview').key1
    'Subscriptions:Staging:Cache:BlobStorage:ConnectionString': 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
    'Subscriptions:Testing:Cache:BlobStorage:ConnectionString': 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
    'Subscriptions:Staging:Cache:BlobStorage:ContainerName': stageSubContainerName
    'Subscriptions:Testing:Cache:BlobStorage:ContainerName': testSubContainerName
    'PublisherConfig:Store:BlobStorage:ConnectionString': 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
    'PublisherConfig:Store:BlobStorage:ContainerName': configContainerName
  }
}

resource logicApps_eventGridConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: logicApps.eventGridConnectionName
  location: location

  properties: {
    displayName: logicApps_ui[language].eventGridConnectionDisplayName
    parameterValues: {
      'token:clientId': aadClientId
      'token:clientSecret': aadClientSecret
      'token:TenantId': aadTenantId
      'token:grantType': 'client_credentials'
    }
    api: {
      id: subscriptionResourceId('Microsoft.Web/locations/managedApis', location, 'azureeventgrid')
    }
  }
  dependsOn: [
    eventGridTopic
  ]
}

output deploymentName string = cleanDeploymentName
output storageAccountName string = storageAccountName
output webAppBaseUrl string = 'https://${webAppName}.azurewebsites.net'
output webAppName string = webAppName
output eventGridTopicName string = eventGridTopicName
output eventGridConnectionName string = logicApps.eventGridConnectionName
