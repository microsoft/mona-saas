# Contents
* [Introduction](#introduction)
* [Events](#events)
* [Testing Mode](#testing-mode)
* [Integration](#integration) 
* [Language Support](#language-support)
* [Conclusion](#conclusion)
# Introduction

Welcome to the Mona SaaS developer guide. This document serves as a means to assist developers as they integrate Mona into their solution. Mona is an open source project that continues to grow and expand. If you would like to contribute, you can do so following these [instructions](https://github.com/microsoft/mona-saas/blob/main/CONTRIBUTING.md). If there is any feedback on this guide dont hesitate to reach out.   


# Events

Mona is an events driven support system. There are six main events that need to occur in order to integrate with the market place. Each of these operations is exposed to your SaaS application by Mona SaaS through events published to a custom Event Grid topic automatically provisioned during setup. By default, we deploy a set of "stub" Logic Apps into your Azure subscription that are enabled by default and configured to be triggered by these subscription events.

Since Mona SaaS exposes these subscription-related events to your SaaS application through an Event Grid topic, you have lots of options for handling them. Because we're using Event Grid, multiple event subscribers can handle the same events simultaneously. These flows can be easily modified in production with no downtime.

## Event Types
* [BaseSubscriptionEvent](basesubscriptionevent)
* [SubscriptionCancelled](subscriptionCancelled)
* [SubscriptionPlanChanged](subscriptionplanchanged)
* [SubscriptionPurchased](subscriptionpurchased)
* [SubscriptionReinstated](subscriptionreinstated)
* [SubscriptionSeatQuantityChanged](subscriptionseatquantitychanged)
* [SubscriptionSuspended](subscriptionsuspended)


#### BaseSubscriptionEvent

This is the base subscription event that all the subsequent events will be based of off. All events following this one will use these parameters **in addiiton** to whatever other parameters are shown in the JSON template. Active (Subscribed) is the steady state of a provisioned SaaS subscription. After the Microsoft side has processed the Activate Subscription API call, the SaaS subscription is marked as Subscribed. The customer can now use the SaaS service on the publisher's side and will be billed.


#### SubscriptionCancelled

Subscriptions reach this state in response to an explicit customer or CSP action by the cancellation of a subscription from the publisher site, the Azure portal, or Microsoft 365 Admin Center. A subscription can also be canceled implicitly, as a result of nonpayment of dues, after being in the Suspended state for 30 days.

#### SubscriptionPlanChanged
Use this API to update (increase or decrease) the quantity of seats purchased for a SaaS subscription. The publisher must call this API when the number of seats is changed from the publisher side for a SaaS subscription created in the commercial marketplace.

#### SubscriptionPurchased
After an end user (or CSP) purchases a SaaS offer in the commercial marketplace, the publisher should be notified of the purchase. The publisher can then create and configure a new SaaS account on the publisher side for the end user.

#### SubscriptionReinstated
This action indicates that the customer's payment instrument has become valid again, a payment has been made for the SaaS subscription, and the subscription is being reinstated. In this case:

### SubscriptionSeatQuantityChanged
Use this API to update (increase or decrease) the quantity of seats purchased for a SaaS subscription. The publisher must call this API when the number of seats is changed from the publisher side for a SaaS subscription created in the commercial marketplace.

#### SubscriptionSuspended
This state indicates that a customer's payment for the SaaS service has not been received. The publisher will be notified of this change in the SaaS subscription status by Microsoft. The notification is done via a call to webhook with the action parameter set to Suspended.

Below is a sample of what the JSON body will look like 

```json
{

  "id": "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
  "subject": "mona/saas/subscriptions/YYYYYYYY-YYYY-YYYY-YYYY-YYYYYYYYYYYY",
  "data": {
    "eventId": "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
    "eventType": "Mona.SaaS.Marketplace.SubscriptionPurchased",
    "eventVersion": "2021-05-01",
    "operationId": "ZZZZZZZZ-ZZZZ-ZZZZ-ZZZZ-ZZZZZZZZZZZZZ",
    "subscription": {
      "subscriptionId": "YYYYYYYY-YYYY-YYYY-YYYY-YYYYYYYYYYYY",
      "subscriptionName": "Test Subscription",
      "offerId": "Test Offer",
      "planId": "Test Plan",
      "isTest": true,
      "isFreeTrial": false,
      "status": 2,
      "term": {
        "termUnit": "PT1M",
        "startDate": "2021-05-26T00:00:00Z",
        "endDate": "2021-06-26T00:00:00Z"
      },

      "beneficiary": {
        "userId": "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA",
        "userEmail": "beneficiary@microsoft.com",
        "aadObjectId": "BBBBBBBB-BBBB-BBBB-BBBBBBBBBBBBBB",
        "aadTenantId": "CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"
      },

      "purchaser": {
        "userId": "DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDDDD",
        "userEmail": "purchaser@microsoft.com",
        "aadObjectId": "EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE",
        "aadTenantId": "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFFF"
      }
    },

    "operationDateTimeUtc": "2021-05-26T14:20:58.7727333Z"
  },
  "eventType": "Mona.SaaS.Marketplace.SubscriptionPurchased",
  "dataVersion": "2021-05-01",
  "metadataVersion": "1",
  "eventTime": "2021-05-26T14:20:58.7806573Z",
  "topic": "/subscriptions/GGGGGGGG-GGGG-GGGG-GGGG-GGGGGGGGGGGG/resourceGroups/monatest10/providers/Microsoft.EventGrid/topics/mona-events-monatest10"
}

```


Parameter | Value
------------ | -------------
id| purchased SaaS subscription ID
subject | Content in the second column
data| 
eventId | same as id
eventType| one of the 6 event types shown above 
eventVersion | date time value
operationId| Content from cell 2
subscription |
subscriptionId | Content in the second column
subscriptionName | Content in the second column
offerId | Content in the second column
planId |  purchased plan, cannot be empty
isTest| Content from cell 2
isFreeTrial | Content in the second column
status | Content from cell 2
term | 
termUnit| Content from cell 2
startDate | Content in the second column
endDate | Content from cell 2
beneficiary | email address, user ID and tenant ID for which SaaS subscription was purchased.
purchaser | 
userId | userId used to 
userEmail | Content in the second column
aadObjectId | Content in the second column
aadTenantId | Content in the second column
operationDateTimeUtc| Content in the second column
eventType | Content in the second column
dataVersion | Content in the second column
metadataVersion | Content in the second column
eventTime | Content in the second column
topic | Content in the second column


# Testing Mode

# Integration

Mona SaaS exposes  subscription-related events to your SaaS application through an Event Grid topic. Mona provides the flexibility for integrating multiple options for handling of the subscription-related events. A subscription tells Event Grid which events on a topic you're interested in receiving. When creating the subscription, you provide an endpoint for handling these event. This section serves as a guidline to help with integrating events handlers in Mona. Outlines below ae example event handlers that can be used to integrate events. Please see the following link for more information on other services that can be used to integrate with Event Grid Topics; [Event Handlers](https://docs.microsoft.com/en-us/azure/event-grid/overview#event-handlers).

Please see below json for Subscription Event Grid Architecture 

```json
{
    "properties": {
        "provisioningState": "Succeeded",
        "endpoint": "https://mona-events-monadocs01.northeurope-1.eventgrid.azure.net/api/events",
        "inputSchema": "EventGridSchema",
        "metricResourceId": "c763c70e-ddf6-4562-b809-02d563077fae",
        "publicNetworkAccess": "Enabled"
    },
    "sku": {
        "name": "Basic"
    },
    "kind": "Azure",
    "systemData": null,
    "location": "northeurope",
    "tags": null,
    "id": "/subscriptions/4ff7b032-cdca-461c-adaf-30be3a1769b3/resourceGroups/mona-monadocs01/providers/Microsoft.EventGrid/topics/mona-events-monadocs01",
    "name": "mona-events-monadocs01",
    "type": "Microsoft.EventGrid/topics"
}
```

## Event Handlers
An [event handler](https://docs.microsoft.com/en-us/azure/event-grid/concepts#event-handlers) is the place where the events are sent. The handler takes an action to process the event. Listed below are a few Azure services that are automatically configured to handle events with the Event Grid. Before integrating please review the supported event handlers for [Azure Event Grid](https://docs.microsoft.com/en-us/azure/event-grid/event-handlers).

#### Azure Functions 
Serverless solution for processing events with minmal code required. For more information, please review the following [docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview).

The following are approaches to using a function as an event handler for [Event Grid Trigger](https://docs.microsoft.com/en-us/azure/event-grid/handler-functions)


#### Logic Apps
Service that helps with automating and orchestrating tasks, business processes and workflows. For more information, please review the following [docs](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-overview). Currenlty Mona has Logic Apps preconfigured out of the box. Please see the below image for reference of Logic App integration in Mona.

![Logic App Architecture](docs/images/monalogicapp.png)

- #### Other Event Grid Subscribers
	- Webhooks
	- Event hubs
	- Relay hybrid connections
	- Service Bus queues and topics
	- Storage queues

## Logic App Integration Logic

![Logic App Integration Logic](docs/images/monalogicappintegration.png)

# Language Support

## Globalization and localization in ASP.NET Core

This application allows multilingual site to reach a wider audience. 
We used ASP.NET Core that provides services and middleware for localizing into different languages and cultures.

Globalization is the process of designing apps that support different cultures. Globalization adds support for input, display, and output of a defined set of language scripts that relate to specific geographic areas.

Localization is the process of adapting a globalized app, which you have already processed for localizability, to a particular culture/locale. For more information see Globalization and localization terms near the end of this document.

**Right now the application support English and Spanish.**

#### Add New languages

1. You need to specify the languages or cultures that you plan to support in the application, go to the `Startup.cs` file and add the language to be supported un the `supportCulture` variable.
   
   You will be able to find how to specific the culture [here](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=net-5.0)
   
   ```
    private void ConfigureLocalizationMiddleware(IApplicationBuilder app)
        {
            var supportedCultures = new[] { "en-US", "es" }; // TODO: Support for additional languages coming soon!

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);
        }
    ```
2. Under the `Resources/Views` folder you will find `.resx` resource files for each view that will contain the translated strings. All views allow to translated the content. You will able to find the `@Localizer[]` tag and the content inside, that will be the content to be translated.

    To create new Resources please follow this [steps](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-5.0#resource-files-2)


## ARM template language configuration

You can configurate the preferred UI language for the Mona deployment. The ARM template support the configuration of different languages for the logic app variables.

#### Add new languages

Under `Mona.SaaS.Setup/arm-templates/basic-deploy.json` you will find the language parameter where you add add a support languages. You will do it under `allowedValues`

    ```json
    "language": {
        "type": "string",
        "defaultValue": "en",
        "allowedValues": [
            "en", // English
            "es" // Spanish
        ],
        ...
    }
    ```

#### Logic Apps variables

After adding the languages you have to add the translation of each string under `logicApps_ui` variables.

Here an example:

    ```json
    "logicApps_ui": {
        "en":{
            "eventGridConnectionDisplayName": "Mona Subscription Events",
            "appNames": {
                "onCancelled": "On subscription cancelled",
                "onPurchased": "On subscription purchased",
                "onPlanChanged": "On subscription plan changed",
                "onSeatQtyChanged": "On subscription seat quantity changed",
                "onSuspended": "On subscription suspended",
                "onReinstated": "On subscription reinstated"
            }
        },
        "es": {
            "eventGridConnectionDisplayName": "Eventos de Suscripción de Mona",
            "appNames": {
                "onCancelled": "Suscripción cancelada",
                "onPurchased": "Suscripción comprada ",
                "onPlanChanged": "Plan de suscripción cambiado",
                "onSeatQtyChanged": "Cantidad de asientos de suscripción cambiada",
                "onSuspended": "Suscripción suspendida",
                "onReinstated": "Suscripción reestablecida"
                }
        }
    ```

# Conclusion

This document provide a summary of the building blocks used in the Mona SaaS project. This should have provided a foundation of the most important part in the Project like Events and the different integration part.

This is an open-source project that speed up the development for our ISV partners to rapidly onboard their transactable SaaS solutions. While we focused on the flows that are required by our own SaaS fulfillment APIs including both the landing page that customers will see when purchasing your SaaS offer and the webhook that we use to notify you of subscription changes like cancellations and suspensions. We will continue working to introduce new features and capabilities. **We encourage you to provide feedback to help us  evolution of this environment.**
