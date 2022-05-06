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

var monaVersion = '0.1-prerelease'
var deploymentName_var = toLower((empty(deploymentName) ? uniqueString(resourceGroup().id) : deploymentName))
var deploymentNameUnique = uniqueString(resourceGroup().id, deployment().name, deploymentName_var)
var storageAccountName_var = 'monastorage${deploymentNameUnique}'
var blobServiceName = 'default'
var configContainerName = 'configuration'
var testSubContainerName = 'test-subscriptions'
var stageSubContainerName = 'stage-subscriptions'
var akvName = 'mona-akv-${deploymentName_var}'
var eventGridTopicName_var = 'mona-events-${deploymentName_var}'
var appInsightsName_var = 'mona-app-insights-${deploymentName_var}'
var appPlanName_var = 'mona-plan-${deploymentName_var}'
var webAppName_var = 'mona-web-${deploymentName_var}'
var marketplaceApiAuthAudience = '20e940b3-4c77-4b0b-9a53-9e16a1b010a7'
var logicApps_prefixes = {
  onPurchased: 'mona-on-subscription-purchased-'
  onCancelled: 'mona-on-subscription-cancelled-'
  onPlanChanged: 'mona-on-subscription-plan-changed-'
  onSeatQtyChanged: 'mona-on-subscription-seat-qty-changed-'
  onSuspended: 'mona-on-subscription-suspended-'
  onReinstated: 'mona-on-subscription-reinstated-'
  onRenewed: 'mona-on-subscription-renewed-'
}
var logicApps_ui = {
  en: {
    eventGridConnectionDisplayName: 'Mona Subscription Events'
    appNames: {
      onCancelled: 'On subscription cancelled'
      onPurchased: 'On subscription purchased'
      onPlanChanged: 'On subscription plan changed'
      onSeatQtyChanged: 'On subscription seat quantity changed'
      onSuspended: 'On subscription suspended'
      onReinstated: 'On subscription reinstated'
      onRenewed: 'On subscription renewed'
    }
    appDescriptions: {
      onCancelled: 'This workflow is automatically triggered after Mona has been notified by the AppSource/Marketplace that a subscription has been cancelled.'
      onPurchased: 'This workflow is automatically triggered after a customer has confirmed their AppSource/Marketplace purchase through the Mona landing page.'
      onPlanChanged: 'This workflow is automatically triggered after Mona has been notified by the AppSource/Marketplace that a subscription plan has been changed.'
      onSeatQtyChanged: 'This workflow is automatically triggered after Mona has been notified by the AppSource/Marketplace that a subscription seat quantity has been changed.'
      onSuspended: 'This workflow is automatically triggered after Mona has been notified by the AppSource/Marketplace that a subscription has been suspended.'
      onReinstated: 'This workflow is automatically triggered after Mona has been notified by the AppSource/Marketplace that a subscription has been reinstated.'
      onRenewed: 'This workflow is automatically triggered after Mona has been notified by the AppSource/Marketplace that a subscription has been renewed.'
    }
    triggerNames: {
      onCancelled: 'When_a_subscription_is_cancelled'
      onPurchased: 'When_a_subscription_is_purchased'
      onPlanChanged: 'When_a_subscription_plan_is_changed'
      onSeatQtyChanged: 'When_subscription_seats_are_changed'
      onSuspended: 'When_a_subscription_is_suspended'
      onReinstated: 'When_a_subscription_is_reinstated'
      onRenewed: 'When_a_subscription_is_renewed'
    }
    actionNames: {
      parseEventInfo: 'Parse_event_information'
      parseSubscriptionInfo: 'Parse_subscription_information'
      yourIntegrationLogic: 'Add_your_integration_logic_here'
      notifyMarketplaceCondition: 'Conditional_|_Notify_the_Marketplace'
      notifyMarketplace: 'Notify_the_Marketplace'
    }
  }
}
var logicApps = {
  eventGridConnectionName: 'mona-events-connection-${deploymentName_var}'
  onPurchased: {
    name: concat(logicApps_prefixes.onPurchased, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onPurchased, deploymentName_var), logicApps_ui[language].triggerNames.onPurchased)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onPurchased, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionPurchased'
  }
  onCancelled: {
    name: concat(logicApps_prefixes.onCancelled, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onCancelled, deploymentName_var), logicApps_ui[language].triggerNames.onCancelled)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onCancelled, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionCancelled'
  }
  onPlanChanged: {
    name: concat(logicApps_prefixes.onPlanChanged, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onPlanChanged, deploymentName_var), logicApps_ui[language].triggerNames.onPlanChanged)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onPlanChanged, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionPlanChanged'
  }
  onSeatQtyChanged: {
    name: concat(logicApps_prefixes.onSeatQtyChanged, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onSeatQtyChanged, deploymentName_var), logicApps_ui[language].triggerNames.onSeatQtyChanged)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onSeatQtyChanged, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionSeatQuantityChanged'
  }
  onSuspended: {
    name: concat(logicApps_prefixes.onSuspended, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onSuspended, deploymentName_var), logicApps_ui[language].triggerNames.onSuspended)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onSuspended, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionSuspended'
  }
  onReinstated: {
    name: concat(logicApps_prefixes.onReinstated, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onReinstated, deploymentName_var), logicApps_ui[language].triggerNames.onReinstated)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onReinstated, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionReinstated'
  }
  onRenewed: {
    name: concat(logicApps_prefixes.onRenewed, deploymentName_var)
    triggerId: resourceId('Microsoft.Logic/workflows/triggers', concat(logicApps_prefixes.onRenewed, deploymentName_var), logicApps_ui[language].triggerNames.onReinstated)
    resourceId: resourceId('Microsoft.Logic/workflows', concat(logicApps_prefixes.onRenewed, deploymentName_var))
    defaultState: 'Enabled'
    triggerEventType: 'Mona.SaaS.Marketplace.SubscriptionRenewed'
  }
}

resource appInsightsName 'microsoft.insights/components@2020-02-02-preview' = {
  name: appInsightsName_var
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    IngestionMode: 'ApplicationInsights'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource storageAccountName 'Microsoft.Storage/storageAccounts@2021-01-01' = {
  name: storageAccountName_var
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource storageAccountName_default 'Microsoft.Storage/storageAccounts/managementPolicies@2019-06-01' = {
  parent: storageAccountName
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

resource storageAccountName_blobServiceName 'Microsoft.Storage/storageAccounts/blobServices@2021-01-01' = {
  parent: storageAccountName
  name: '${blobServiceName}'
  properties: {
    isVersioningEnabled: true
  }
}

resource storageAccountName_blobServiceName_testSubContainerName 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-01-01' = {
  parent: storageAccountName_blobServiceName
  name: testSubContainerName
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccountName
  ]
}

resource storageAccountName_blobServiceName_stageSubContainerName 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-01-01' = {
  parent: storageAccountName_blobServiceName
  name: stageSubContainerName
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccountName
  ]
}

resource storageAccountName_blobServiceName_configContainerName 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-01-01' = {
  parent: storageAccountName_blobServiceName
  name: configContainerName
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccountName
  ]
}

resource eventGridTopicName 'Microsoft.EventGrid/topics@2020-06-01' = {
  name: eventGridTopicName_var
  location: resourceGroup().location
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
  }
}

resource appPlanName 'Microsoft.Web/serverfarms@2018-02-01' = if (empty(appServicePlanId)) {
  name: appPlanName_var
  location: resourceGroup().location
  sku: {
    name: 'S1'
    tier: 'Standard'
    size: 'S1'
    family: 'S'
    capacity: 1
  }
  kind: 'app'
}

resource webAppName 'Microsoft.Web/sites@2018-11-01' = {
  name: webAppName_var
  location: resourceGroup().location
  kind: 'app'
  properties: {
    serverFarmId: (empty(appServicePlanId) ? appPlanName.id : appServicePlanId)
  }
  dependsOn: [
    appInsightsName
  ]
}

resource webAppName_appsettings 'Microsoft.Web/sites/config@2020-12-01' = {
  parent: webAppName
  name: 'appsettings'
  properties: {
    'Deployment:AppInsightsInstrumentationKey': reference(appInsightsName.id, '2014-04-01').InstrumentationKey
    'Deployment:AzureResourceGroupName': resourceGroup().name
    'Deployment:AzureSubscriptionId': subscription().subscriptionId
    'Deployment:EventVersion': eventVersion
    'Deployment:IsTestModeEnabled': 'true'
    'Deployment:MonaVersion': monaVersion
    'Deployment:Name': deploymentName_var
    'Identity:AdminIdentity:AadTenantId': aadTenantId
    'Identity:AdminIdentity:RoleName': 'monaadmins'
    'Identity:AppIdentity:AadClientId': aadClientId
    'Identity:AppIdentity:AadClientSecret': aadClientSecret
    'Identity:AppIdentity:AadPrincipalId': aadPrincipalId
    'Identity:AppIdentity:AadTenantId': aadTenantId
    'Subscriptions:Events:EventGrid:TopicEndpoint': eventGridTopicName.properties.endpoint
    'Subscriptions:Events:EventGrid:TopicKey': listKeys(eventGridTopicName.id, '2020-04-01-preview').key1
    'Subscriptions:Staging:Cache:BlobStorage:ConnectionString': 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName_var};AccountKey=${listKeys(storageAccountName.id, '2021-01-01').keys[0].value};EndpointSuffix=core.windows.net'
    'Subscriptions:Testing:Cache:BlobStorage:ConnectionString': 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName_var};AccountKey=${listKeys(storageAccountName.id, '2021-01-01').keys[0].value};EndpointSuffix=core.windows.net'
    'Subscriptions:Staging:Cache:BlobStorage:ContainerName': stageSubContainerName
    'Subscriptions:Testing:Cache:BlobStorage:ContainerName': testSubContainerName
    'PublisherConfig:Store:BlobStorage:ConnectionString': 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName_var};AccountKey=${listKeys(storageAccountName.id, '2021-01-01').keys[0].value};EndpointSuffix=core.windows.net'
    'PublisherConfig:Store:BlobStorage:ContainerName': configContainerName
  }
}

resource logicApps_eventGridConnectionName 'Microsoft.Web/connections@2016-06-01' = {
  name: logicApps.eventGridConnectionName
  location: resourceGroup().location
  kind: 'V1'
  properties: {
    displayName: logicApps_ui[language].eventGridConnectionDisplayName
    customParameterValues: {}
    parameterValues: {
      'token:clientId': aadClientId
      'token:clientSecret': aadClientSecret
      'token:TenantId': aadTenantId
      'token:grantType': 'client_credentials'
    }
    api: {
      id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
    }
  }
  dependsOn: [
    eventGridTopicName
  ]
}

resource logicApps_onPurchased_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onPurchased.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onPurchased
    'mona:description': logicApps_ui[language].appDescriptions.onPurchased
    'mona:event-type': logicApps.onPurchased.triggerEventType
  }
  properties: {
    state: logicApps.onPurchased.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onPurchased}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'WebHook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onPurchased.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
        '${logicApps_ui[language].actionNames.notifyMarketplaceCondition}': {
          actions: {
            '${logicApps_ui[language].actionNames.notifyMarketplace}': {
              inputs: {
                authentication: {
                  audience: marketplaceApiAuthAudience
                  clientId: aadClientId
                  secret: aadClientSecret
                  tenant: aadTenantId
                  type: 'ActiveDirectoryOAuth'
                }
                body: {
                  planId: '@{body(\'Parse_subscription_information\')?[\'Plan ID\']}'
                  quantity: '@{body(\'Parse_subscription_information\')?[\'Seat Quantity\']}'
                }
                headers: {
                  'content-type': 'application/json'
                }
                method: 'POST'
                uri: 'https://marketplaceapi.microsoft.com/api/saas/subscriptions/@{body(\'Parse_subscription_information\')?[\'Subscription ID\']}/activate?api-version=2018-08-31'
              }
              runAfter: {}
              type: 'Http'
            }
          }
          expression: {
            and: [
              {
                equals: [
                  true
                  false
                ]
              }
              {
                equals: [
                  '@body(\'Parse_subscription_information\')?[\'Is Test Subscription?\']'
                  false
                ]
              }
            ]
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.yourIntegrationLogic}': [
              'Succeeded'
            ]
          }
          type: 'If'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

resource logicApps_onCancelled_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onCancelled.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onCancelled
    'mona:description': logicApps_ui[language].appDescriptions.onCancelled
    'mona:event-type': logicApps.onCancelled.triggerEventType
  }
  properties: {
    state: logicApps.onCancelled.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onCancelled}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'webhook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onCancelled.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

resource logicApps_onRenewed_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onRenewed.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onRenewed
    'mona:description': logicApps_ui[language].appDescriptions.onRenewed
    'mona:event-type': logicApps.onCancelled.triggerEventType
  }
  properties: {
    state: logicApps.onRenewed.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onRenewed}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'webhook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onRenewed.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

resource logicApps_onPlanChanged_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onPlanChanged.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onPlanChanged
    'mona:description': logicApps_ui[language].appDescriptions.onPlanChanged
    'mona:event-type': logicApps.onPlanChanged.triggerEventType
  }
  properties: {
    state: logicApps.onPlanChanged.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onPlanChanged}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'webhook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onPlanChanged.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
                'New Plan ID': {
                  type: 'string'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
        '${logicApps_ui[language].actionNames.notifyMarketplaceCondition}': {
          actions: {
            '${logicApps_ui[language].actionNames.notifyMarketplace}': {
              inputs: {
                authentication: {
                  audience: marketplaceApiAuthAudience
                  clientId: aadClientId
                  secret: aadClientSecret
                  tenant: aadTenantId
                  type: 'ActiveDirectoryOAuth'
                }
                body: {
                  status: 'Success'
                }
                headers: {
                  'content-type': 'application/json'
                }
                method: 'PATCH'
                uri: 'https://marketplaceapi.microsoft.com/api/saas/subscriptions/@{body(\'Parse_subscription_information\')?[\'Subscription ID\']}/operations/@{body(\'Parse_event_information\')?[\'Operation ID\']}?api-version=2018-08-31'
              }
              runAfter: {}
              type: 'Http'
            }
          }
          expression: {
            and: [
              {
                equals: [
                  true
                  false
                ]
              }
              {
                equals: [
                  '@body(\'Parse_subscription_information\')?[\'Is Test Subscription?\']'
                  false
                ]
              }
            ]
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.yourIntegrationLogic}': [
              'Succeeded'
            ]
          }
          type: 'If'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

resource logicApps_onSeatQtyChanged_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onSeatQtyChanged.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onSeatQtyChanged
    'mona:description': logicApps_ui[language].appDescriptions.onSeatQtyChanged
    'mona:event-type': logicApps.onSeatQtyChanged.triggerEventType
  }
  properties: {
    state: logicApps.onSeatQtyChanged.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onSeatQtyChanged}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'webhook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onSeatQtyChanged.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
                'New Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
        '${logicApps_ui[language].actionNames.notifyMarketplaceCondition}': {
          actions: {
            '${logicApps_ui[language].actionNames.notifyMarketplace}': {
              inputs: {
                authentication: {
                  audience: marketplaceApiAuthAudience
                  clientId: aadClientId
                  secret: aadClientSecret
                  tenant: aadTenantId
                  type: 'ActiveDirectoryOAuth'
                }
                body: {
                  status: 'Success'
                }
                headers: {
                  'content-type': 'application/json'
                }
                method: 'PATCH'
                uri: 'https://marketplaceapi.microsoft.com/api/saas/subscriptions/@{body(\'Parse_subscription_information\')?[\'Subscription ID\']}/operations/@{body(\'Parse_event_information\')?[\'Operation ID\']}?api-version=2018-08-31'
              }
              runAfter: {}
              type: 'Http'
            }
          }
          expression: {
            and: [
              {
                equals: [
                  true
                  false
                ]
              }
              {
                equals: [
                  '@body(\'Parse_subscription_information\')?[\'Is Test Subscription?\']'
                  false
                ]
              }
            ]
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.yourIntegrationLogic}': [
              'Succeeded'
            ]
          }
          type: 'If'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

resource logicApps_onSuspended_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onSuspended.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onSuspended
    'mona:description': logicApps_ui[language].appDescriptions.onSuspended
    'mona:event-type': logicApps.onSuspended.triggerEventType
  }
  properties: {
    state: logicApps.onSuspended.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onSuspended}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'webhook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onSuspended.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

resource logicApps_onReinstated_name 'Microsoft.Logic/workflows@2017-07-01' = {
  name: logicApps.onReinstated.name
  location: resourceGroup().location
  tags: {
    'mona:name': logicApps_ui[language].appNames.onReinstated
    'mona:description': logicApps_ui[language].appDescriptions.onReinstated
    'mona:event-type': logicApps.onReinstated.triggerEventType
  }
  properties: {
    state: logicApps.onReinstated.defaultState
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {}
          type: 'Object'
        }
      }
      triggers: {
        '${logicApps_ui[language].triggerNames.onReinstated}': {
          splitOn: '@triggerBody()'
          type: 'ApiConnectionWebhook'
          inputs: {
            body: {
              properties: {
                destination: {
                  endpointType: 'webhook'
                  properties: {
                    endpointUrl: '@{listCallbackUrl()}'
                  }
                }
                filter: {
                  includedEventTypes: [
                    logicApps.onReinstated.triggerEventType
                  ]
                }
                topic: eventGridTopicName.id
              }
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'eventGrid\'][\'connectionId\']'
              }
            }
            path: '${subscription().id}/providers/Microsoft.EventGrid.Topics/resource/eventSubscriptions'
            queries: {
              'x-ms-api-version': '2017-06-15-preview'
            }
          }
        }
      }
      actions: {
        '${logicApps_ui[language].actionNames.parseEventInfo}': {
          inputs: {
            content: '@triggerBody()?[\'data\']'
            schema: {
              properties: {
                'Event ID': {
                  type: 'string'
                }
                'Event Type': {
                  type: 'string'
                }
                'Event Version': {
                  type: 'string'
                }
                'Operation Date/Time UTC': {
                  type: 'string'
                }
                'Operation ID': {
                  type: 'string'
                }
                Subscription: {
                  type: 'object'
                }
              }
              type: 'object'
            }
          }
          runAfter: {}
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': {
          inputs: {
            content: '@body(\'Parse_event_information\')?[\'Subscription\']'
            schema: {
              properties: {
                'Beneficiary AAD Object ID': {
                  type: 'string'
                }
                'Beneficiary AAD Tenant ID': {
                  type: 'string'
                }
                'Beneficiary Email Address': {
                  type: 'string'
                }
                'Beneficiary User ID': {
                  type: 'string'
                }
                'Is Free Trial Subscription?': {
                  type: 'boolean'
                }
                'Is Test Subscription?': {
                  type: 'boolean'
                }
                'Offer ID': {
                  type: 'string'
                }
                'Plan ID': {
                  type: 'string'
                }
                'Purchaser AAD Object ID': {
                  type: 'string'
                }
                'Purchaser AAD Tenant ID': {
                  type: 'string'
                }
                'Purchaser Email Address': {
                  type: 'string'
                }
                'Purchaser User ID': {
                  type: 'string'
                }
                'Subscription End Date': {
                  type: 'string'
                }
                'Subscription ID': {
                  type: 'string'
                }
                'Subscription Name': {
                  type: 'string'
                }
                'Subscription Start Date': {
                  type: 'string'
                }
                'Subscription Status': {
                  type: 'string'
                }
                'Subscription Term Unit': {
                  type: 'string'
                }
                'Seat Quantity': {
                  type: 'number'
                }
              }
              type: 'object'
            }
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.parseEventInfo}': [
              'Succeeded'
            ]
          }
          type: 'ParseJson'
        }
        '${logicApps_ui[language].actionNames.yourIntegrationLogic}': {
          actions: {}
          runAfter: {
            '${logicApps_ui[language].actionNames.parseSubscriptionInfo}': [
              'Succeeded'
            ]
          }
          type: 'Scope'
        }
        '${logicApps_ui[language].actionNames.notifyMarketplaceCondition}': {
          actions: {
            '${logicApps_ui[language].actionNames.notifyMarketplace}': {
              inputs: {
                authentication: {
                  audience: marketplaceApiAuthAudience
                  clientId: aadClientId
                  secret: aadClientSecret
                  tenant: aadTenantId
                  type: 'ActiveDirectoryOAuth'
                }
                body: {
                  status: 'Success'
                }
                headers: {
                  'content-type': 'application/json'
                }
                method: 'PATCH'
                uri: 'https://marketplaceapi.microsoft.com/api/saas/subscriptions/@{body(\'Parse_subscription_information\')?[\'Subscription ID\']}/operations/@{body(\'Parse_event_information\')?[\'Operation ID\']}?api-version=2018-08-31'
              }
              runAfter: {}
              type: 'Http'
            }
          }
          expression: {
            and: [
              {
                equals: [
                  true
                  false
                ]
              }
              {
                equals: [
                  '@body(\'Parse_subscription_information\')?[\'Is Test Subscription?\']'
                  false
                ]
              }
            ]
          }
          runAfter: {
            '${logicApps_ui[language].actionNames.yourIntegrationLogic}': [
              'Succeeded'
            ]
          }
          type: 'If'
        }
      }
    }
    parameters: {
      '$connections': {
        value: {
          eventGrid: {
            connectionId: logicApps_eventGridConnectionName.id
            connectionName: logicApps.eventGridConnectionName
            id: '${subscription().id}/providers/Microsoft.Web/locations/${resourceGroup().location}/managedApis/azureeventgrid'
          }
        }
      }
    }
  }
}

output deploymentName string = deploymentName_var
output storageAccountName string = storageAccountName_var
output webAppBaseUrl string = 'https://${webAppName_var}.azurewebsites.net'
output webAppName string = webAppName_var