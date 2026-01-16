@description('A unique, publisher-defined name that identifies this Mona deployment.')
@maxLength(13)
param deploymentName string = ''

@description('This Mona deployment\'s admin web app Entra tenant ID.')
param aadTenantId string

@description('This Mona deployment\'s admin web app Entra client ID.')
param aadClientId string

@description('If provided, specifies the ID of an existing app service plan that the Mona web app should be deployed to.')
param appServicePlanId string = ''

@description('The version of events that this Mona deployment will publish to Event Grid.')
@allowed([
  '2021-05-01'
  '2021-10-01'
])
param eventVersion string = '2021-10-01'

@description('The version of Mona that this deployment is running.')
param monaVersion string

@description('The location where the resources in this Mona deployment should be created.')
param location string = resourceGroup().location

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

var adminWebAppName = 'mona-admin-${cleanDeploymentName}'
var customerWebAppName = 'mona-customer-${cleanDeploymentName}'

var externalMidName = 'mona-external-id-${cleanDeploymentName}'
var internalMidName = 'mona-internal-id-${cleanDeploymentName}'

var eventGridConnectionName = 'mona-eventgrid-connection-${cleanDeploymentName}'
var eventGridConnectionDisplayName = 'Mona SaaS Subscription Events'

resource externalMid 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: externalMidName
  location: location
}

resource internalMid 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: internalMidName
  location: location
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

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
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

resource eventGridTopic 'Microsoft.EventGrid/topics@2024-06-01-preview' = {
  name: eventGridTopicName
  location: location
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }
}

resource appPlan 'Microsoft.Web/serverfarms@2018-02-01' = if (empty(appServicePlanId)) {
  name: appPlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  properties: {
  }
  kind: 'app'
}

resource adminWebApp 'Microsoft.Web/sites@2018-11-01' = {
  name: adminWebAppName
  location: location
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${externalMid.id}': {}
      '${internalMid.id}': {}
    }
  }
  properties: {
    serverFarmId: (empty(appServicePlanId) ? appPlan.id : appServicePlanId)
  }
  dependsOn: [
    appInsights
  ]
}

resource customerWebApp 'Microsoft.Web/sites@2018-11-01' = {
  name: customerWebAppName
  location: location
  kind: 'app'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${externalMid.id}': {}
      '${internalMid.id}': {}
    }
  }
  properties: {
    serverFarmId: (empty(appServicePlanId) ? appPlan.id : appServicePlanId)
  }
  dependsOn: [
    appInsights
  ]
}

resource customerWebApp_appsettings 'Microsoft.Web/sites/config@2020-12-01' = {
  parent: customerWebApp
  name: 'appsettings'
  properties: {
    'Deployment:AppInsightsConnectionString': appInsights.properties.ConnectionString
    'Deployment:AzureResourceGroupName': resourceGroup().name
    'Deployment:AzureSubscriptionId': subscription().subscriptionId
    'Deployment:EventVersion': eventVersion
    'Deployment:MonaVersion': monaVersion
    'Deployment:Name': cleanDeploymentName
    'Identity:EntraTenantId': aadTenantId
    'Identity:AdminAppIdentity:EntraAppId': aadClientId
    'Identity:AdminAppIdentity:EntraTenantId': aadTenantId
    'Identity:ManagedIdentities:ExternalManagedId': externalMid.id
    'Identity:ManagedIdentities:InternalManagedId': internalMid.id
    'Marketplace:LandingPageUrl': 'https://${customerWebAppName}.azurewebsites.net'
    'Marketplace:WebhookUrl': 'https://${customerWebAppName}.azurewebsites.net/webhook'
    'Subscriptions:Events:EventGrid:TopicEndpoint': eventGridTopic.properties.endpoint
    'Subscriptions:Staging:Cache:BlobStorage:ContainerName': stageSubContainerName
    'Subscriptions:Staging:Cache:BlobStorage:StorageAccountName': storageAccountName
    'Subscriptions:Testing:Cache:BlobStorage:ContainerName': testSubContainerName
    'Subscriptions:Testing:Cache:BlobStorage:StorageAccountName': storageAccountName
    'PublisherConfig:Store:BlobStorage:ContainerName': configContainerName
    'PublisherConfig:Store:BlobStorage:StorageAccountName': storageAccountName
  }
}

resource adminWebApp_appsettings 'Microsoft.Web/sites/config@2020-12-01' = {
  parent: adminWebApp
  name: 'appsettings'
  properties: {
    'Deployment:AppInsightsConnectionString': appInsights.properties.ConnectionString
    'Deployment:AzureResourceGroupName': resourceGroup().name
    'Deployment:AzureSubscriptionId': subscription().subscriptionId
    'Deployment:EventVersion': eventVersion
    'Deployment:MonaVersion': monaVersion
    'Deployment:Name': cleanDeploymentName
    'Identity:EntraTenantId': aadTenantId
    'Identity:AdminAppIdentity:EntraAppId': aadClientId
    'Identity:AdminAppIdentity:EntraTenantId': aadTenantId
    'Identity:ManagedIdentities:ExternalManagedId': externalMid.id
    'Identity:ManagedIdentities:InternalManagedId': internalMid.id
    'Marketplace:LandingPageUrl': 'https://${customerWebAppName}.azurewebsites.net'
    'Marketplace:WebhookUrl': 'https://${customerWebAppName}.azurewebsites.net/webhook'
    'Subscriptions:Events:EventGrid:TopicEndpoint': eventGridTopic.properties.endpoint
    'Subscriptions:Staging:Cache:BlobStorage:ContainerName': stageSubContainerName
    'Subscriptions:Staging:Cache:BlobStorage:StorageAccountName': storageAccountName
    'Subscriptions:Testing:Cache:BlobStorage:ContainerName': testSubContainerName
    'Subscriptions:Testing:Cache:BlobStorage:StorageAccountName': storageAccountName
    'PublisherConfig:Store:BlobStorage:ContainerName': configContainerName
    'PublisherConfig:Store:BlobStorage:StorageAccountName': storageAccountName
  }
}

resource eventGridConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: eventGridConnectionName
  location: location
  kind: 'V1'
  properties: {
    customParameterValues: {}
    displayName: eventGridConnectionDisplayName
    parameterValueType: 'Alternative'
    api: {
      id: '${subscription().id}/providers/Microsoft.Web/locations/${eventGridTopic.location}/managedApis/azureeventgrid'
    }
  }
}

output deploymentName string = cleanDeploymentName

output storageAccountId string = storageAccount.id
output storageAccountName string = storageAccountName

output adminWebAppUrl string = 'https://${adminWebAppName}.azurewebsites.net'
output adminWebAppName string = adminWebAppName

output customerWebAppUrl string = 'https://${customerWebAppName}.azurewebsites.net'
output customerWebAppName string = customerWebAppName

output eventGridTopicId string = eventGridTopic.id
output eventGridTopicName string = eventGridTopicName
output eventGridConnectionId string = eventGridConnection.id
output eventGridConnectionName string = eventGridConnectionName

output externalMidId string = externalMid.id
output internalMidId string = internalMid.id
output externalMidName string = externalMidName
output internalMidName string = internalMidName
