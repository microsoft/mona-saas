@minLength(5)
@maxLength(13)
@description('''
Deployment name __must__:
- be globally unique;
- contain only alphanumeric characters (a-z, 0-9);
- be at least 5 characters long;
- be less than 13 characters long
''')
param deploymentName string = take(uniqueString(resourceGroup().id), 13)

param monaAdminRole string = 'monaadmins'
param monaAadClientId string
param monaAadTenantId string
param monaAadPrincipalId string
param monaEventVersion string = '2021-10-01'
param monaVersion string

@secure()
param monaAadClientSecret string

param turnPublisherAdminRole string = 'turnstile_admins'
param turnTenantAdminRole string = 'subscriber_tenant_admins'
param turnAadClientId string
param turnAadTenantId string
param turnVersion string

@secure()
param turnAadClientSecret string

param location string = resourceGroup().location

// For shared resources

var cleanDeploymentName = toLower(deploymentName)
var storageAccountName = take('saasstor${uniqueString(resourceGroup().id, cleanDeploymentName)}', 24)
var appInsightsName = 'saas-insights-${cleanDeploymentName}'
var appServicePlanName = 'saas-plan-${cleanDeploymentName}'
var eventGridTopicName = 'saas-events-${cleanDeploymentName}'
var eventGridConnectionName = 'saas-events-connection-${cleanDeploymentName}'
var eventGridConnectionDisplayName = 'SaaS Subscription Events'
var relayApiAppName = 'mona-turn-relay-${cleanDeploymentName}'

// For Mona-specific resources

var monaConfigContainerName = 'mona-configuration'
var monaTestSubContainerName = 'test-subscriptions'
var monaStageSubContainerName = 'stage-subscriptions'
var monaWebAppName = 'mona-web-${cleanDeploymentName}'

// For Turnstile-specific resources

var turnEventStoreContainerName = 'event-store'
var turnConfigContainerName = 'turn-configuration'
var turnConfigBlobName = 'publisher_config.json'
var turnCosmosDbAccountName = 'turn-cosmos-${cleanDeploymentName}'
var turnCosmosDbName = 'turnstiledb'
var turnCosmosContainerName = 'turnstilecontainer'
var turnApiAppName = 'turn-services-${cleanDeploymentName}'
var turnWebAppName = 'turn-web-${cleanDeploymentName}'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    IngestionMode: 'ApplicationInsights'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-11-15-preview' = {
  name: turnCosmosDbAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
      }
    ]
  }
}

resource cosmosSqlDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-11-15-preview' = {
  name: '${cosmosDbAccount.name}/${turnCosmosDbName}'
  properties: {
    resource: {
      id: turnCosmosDbName
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-11-15-preview' = {
  name: '${cosmosSqlDb.name}/${turnCosmosContainerName}'
  properties: {
    resource: {
      id: turnCosmosContainerName
      partitionKey: {
        paths: [
          '/partition_id'
        ]
        kind: 'Hash'
      }
      defaultTtl: -1
    }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
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

resource monaConfigStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${monaConfigContainerName}'
}

resource turnConfigStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${turnConfigContainerName}'
}

resource monaTestSubStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${monaTestSubContainerName}'
}

resource monaStageSubStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${monaStageSubContainerName}'
}

resource turnEventStoreStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${storageAccount.name}/default/${turnEventStoreContainerName}'
}

resource storageAccountPolicies 'Microsoft.Storage/storageAccounts/managementPolicies@2019-06-01' = {
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
                monaStageSubContainerName
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
                monaTestSubContainerName
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

resource eventGridTopic 'Microsoft.EventGrid/topics@2021-12-01' = {
  name: eventGridTopicName
  location: location
  properties: {
    inputSchema: 'EventGridSchema'
  }
}

resource eventGridConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: eventGridConnectionName
  location: location
  properties: {
    displayName: eventGridConnectionDisplayName
    parameterValues: {
      'token:clientId': monaAadClientId
      'token:clientSecret': monaAadClientSecret
      'token:TenantId': monaAadTenantId
      'token:grantType': 'client_credentials'
    }
    api: {
      id: '${subscription().id}/providers/Microsoft.Web/locations/${eventGridTopic.location}/managedApis/azureeventgrid'
    }
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
    family: 'S'
    size: 'S1'
  }
  properties: { }
}

resource relayApiApp 'Microsoft.Web/sites@2021-03-01' = {
  name: relayApiAppName
  location: location
  kind: 'functionapp'
  dependsOn: [
    waitForTurnApiApp
  ]
  properties: {
    reserved: false
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      alwaysOn: true
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
        {
          name: 'AzureWebJobsDashboard'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'Turnstile_ApiAccessKey'
          value: listKeys('${turnApiApp.id}/host/default', turnApiApp.apiVersion).functionKeys.default
        }
        {
          name: 'Turnstile_ApiBaseUrl'
          value: 'https://${turnApiAppName}.azurewebsites.net'
        }
        {
          name: 'Turnstile_EventGridTopicAccessKey'
          value: eventGridTopic.listKeys().key1
        }
        {
          name: 'Turnstile_EventGridTopicEndpointUrl'
          value: eventGridTopic.properties.endpoint
        }
      ]
    }
  }
}

resource turnApiApp 'Microsoft.Web/sites@2021-03-01' = {
  name: turnApiAppName
  location: location
  kind: 'functionapp'
  properties: {
    reserved: false
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      alwaysOn: true
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
        {
          name: 'AzureWebJobsDashboard'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'Turnstile_CosmosAccessKey'
          value: cosmosDbAccount.listKeys().primaryMasterKey
        }
        {
          name: 'Turnstile_CosmosContainerId'
          value: turnCosmosContainerName
        }
        {
          name: 'Turnstile_CosmosDatabaseId'
          value: turnCosmosDbName
        }
        {
          name: 'Turnstile_CosmosEndpointUrl'
          value: cosmosDbAccount.properties.documentEndpoint
        }
        {
          name: 'Turnstile_EventGridTopicAccessKey'
          value: eventGridTopic.listKeys().key1
        }
        {
          name: 'Turnstile_EventGridTopicEndpointUrl'
          value: eventGridTopic.properties.endpoint
        }
        {
          name: 'Turnstile_PublisherConfigStorageBlobName'
          value: turnConfigBlobName
        }
        {
          name: 'Turnstile_PublisherConfigStorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'Turnstile_PublisherConfigStorageContainerName'
          value: turnConfigContainerName
        }
      ]
    }
  }
}

// This is a nasty yet necessary hack. [webApp] is created after [apiApp] because 
// [webApp] references the [apiApp] access key and base URL through it's appsettings. 
// However, there's some "sneak operation" that's eventually consistent following the successful
// deployment of [apiApp] that isn't always complete by the time that [apiApp] deployment 
// is complete (OK status per the ARM API) and causes the listKeys operation inside [webApp] appsettings
// to fail. For that reason, we wait an additional 2 minutes following [apiApp] deployment
// to deploy the [webApp] giving the eventually consistent operation time to complete before
// we try to reference the function keys.

// This appears to work fine and was inspired by this Microsoft escalation engineer tecnical community article -- 
// https://techcommunity.microsoft.com/t5/azure-database-support-blog/add-wait-operation-to-arm-template-deployment/ba-p/2915342

resource waitForTurnApiApp 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'wait-${cleanDeploymentName}'
  location: location
  kind: 'AzurePowerShell'
  dependsOn: [
    turnApiApp
  ]
  properties: {
    azPowerShellVersion: '6.4'
    timeout: 'PT1H'
    scriptContent: 'start-sleep -Seconds 30'
    cleanupPreference: 'Always'
    retentionInterval: 'PT1H'
  }
}

resource monaWebApp 'Microsoft.Web/sites@2021-03-01' = {
  name: monaWebAppName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'Deployment:AppInsightsInstrumentationKey'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'Deployment:AzureResourceGroupName'
          value: resourceGroup().name
        }
        {
          name: 'Deployment:AzureSubscriptionId'
          value: subscription().subscriptionId
        }
        {
          name: 'Deployment:EventVersion'
          value: monaEventVersion
        }
        {
          name: 'Deployment:IsTestModeEnabled'
          value: 'true'
        }
        {
          name: 'Deployment:MonaVersion'
          value: monaVersion
        }
        {
          name: 'Deployment:Name'
          value: cleanDeploymentName
        }
        {
          name: 'Identity:AdminIdentity:AadTenantId'
          value: monaAadTenantId
        }
        {
          name: 'Identity:AdminIdentity:RoleName'
          value: monaAdminRole
        }
        {
          name: 'Identity:AppIdentity:AadClientId'
          value: monaAadClientId
        }
        {
          name: 'Identity:AppIdentity:AadClientSecret'
          value: monaAadClientSecret
        }
        {
          name: 'Identity:AppIdentity:AadPrincipalId'
          value: monaAadPrincipalId
        }
        {
          name: 'Identity:AppIdentity:AadTenantId'
          value: monaAadTenantId
        }
        {
          name: 'Subscriptions:Events:EventGrid:TopicEndpoint'
          value: eventGridTopic.properties.endpoint
        }
        {
          name: 'Subscriptions:Events:EventGrid:TopicKey'
          value: eventGridTopic.listKeys().key1
        }
        {
          name: 'Subscriptions:Staging:Cache:BlobStorage:ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'Subscriptions:Testing:Cache:BlobStorage:ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'Subscriptions:Staging:Cache:BlobStorage:ContainerName'
          value: monaStageSubContainerName
        }
        {
          name: 'Subscriptions:Testing:Cache:BlobStorage:ContainerName'
          value: monaTestSubContainerName
        }
        {
          name: 'PublisherConfig:Store:BlobStorage:ConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'PublisherConfig:Store:BlobStorage:ContainerName'
          value: monaConfigContainerName
        }
      ]
    }
  }
}

resource turnWebApp 'Microsoft.Web/sites@2021-03-01' = {
  name: turnWebAppName
  location: location
  kind: 'app'
  dependsOn: [
    waitForTurnApiApp
  ]
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'Turnstile_ApiAccessKey'
          value: listKeys('${turnApiApp.id}/host/default', turnApiApp.apiVersion).functionKeys.default
        }
        {
          name: 'Turnstile_ApiBaseUrl'
          value: 'https://${turnApiAppName}.azurewebsites.net'
        }
        {
          name: 'Turnstile_PublisherAdminRoleName'
          value: turnPublisherAdminRole
        }
        {
          name: 'Turnstile_AadClientId'
          value: turnAadClientId
        }
        {
          name: 'Turnstile_AadClientSecret'
          value: turnAadClientSecret
        }
        {
          name: 'Turnstile_PublisherTenantId'
          value: turnAadTenantId
        }
        {
          name: 'Turnstile_SubscriberTenantAdminRoleName'
          value: turnTenantAdminRole
        }
      ]
    }
  }
}

output monaWebAppName string = monaWebAppName
output relayName string = relayApiAppName
output turnWebAppName string = turnWebAppName
output turnApiAppName string = turnApiAppName

output relayId string = relayApiApp.id
output topicId string = eventGridTopic.id
output topicName string = eventGridTopic.name
output topicConnectionName string = eventGridConnection.name

output storageAccountName string = storageAccount.name
output storageAccountKey string = storageAccount.listKeys().keys[0].value

output monaPublisherConfig object = {
  SubscriptionConfigurationUrl: 'https://${turnWebAppName}.azurewebsites.net/subscriptions/{subscription-id}'
  SubscriptionPurchaseConfirmationUrl: 'https://${turnWebAppName}.azurewebsites.net/from-mona'
}

output turnPublisherConfig object = {
  is_setup_complete: false
  mona_base_storage_url: storageAccount.properties.primaryEndpoints.blob
  default_seating_config: {
    seating_strategy_name: 'first_come_first_served'
    limited_overflow_seating_enabled: true
    seat_reservation_expiry_in_days: 14
    default_seat_expiry_in_days: 14
  }
}
