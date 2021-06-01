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

```json
{
  
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
    },
    "type": "object"
}
```
#### SubscriptionCancelled

Subscriptions reach this state in response to an explicit customer or CSP action by the cancellation of a subscription from the publisher site, the Azure portal, or Microsoft 365 Admin Center. A subscription can also be canceled implicitly, as a result of nonpayment of dues, after being in the Suspended state for 30 days.

```json
{
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "New Plan ID": {
            "type": "string"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
     },
    "type": "object"
}
```

#### SubscriptionPlanChanged
Use this API to update (increase or decrease) the quantity of seats purchased for a SaaS subscription. The publisher must call this API when the number of seats is changed from the publisher side for a SaaS subscription created in the commercial marketplace.
```json
{
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "New Plan ID": {
            "type": "string"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
    },
    "type": "object"
}
```
#### SubscriptionPurchased
```json
{
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
    },
    "type": "object"
}
```
#### SubscriptionReinstated
This action indicates that the customer's payment instrument has become valid again, a payment has been made for the SaaS subscription, and the subscription is being reinstated. In this case:

```json
{
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
    },
    "type": "object"
}
```
### SubscriptionSeatQuantityChanged
Use this API to update (increase or decrease) the quantity of seats purchased for a SaaS subscription. The publisher must call this API when the number of seats is changed from the publisher side for a SaaS subscription created in the commercial marketplace.
```json
{
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "New Seat Quantity": {
            "type": "integer"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
    },
    "type": "object"
}
```
#### SubscriptionSuspended
This state indicates that a customer's payment for the SaaS service has not been received. The publisher will be notified of this change in the SaaS subscription status by Microsoft. The notification is done via a call to webhook with the action parameter set to Suspended.
```json
{
    "properties": {
        "Is Free Trial Subscription?": {
            "type": "boolean"
        },
        "Is Test Subscription?": {
            "type": "boolean"
        },
        "Offer ID": {
            "type": "string"
        },
        "Operation Date/Time UTC": {
            "type": "string"
        },
        "Operation ID": {
            "type": "string"
        },
        "Plan ID": {
            "type": "string"
        },
        "Seat Quantity": {
            "type": "integer"
        },
        "Subscription ID": {
            "type": "string"
        },
        "Subscription Name": {
            "type": "string"
        }
    },
    "type": "object"
}
```

Parameter | Value
------------ | -------------
Is Free Trial Subscription?| Content from cell 2
Is Test Subscription? | Content in the second column
Offer ID| Content from cell 2
Operation Date/Time UTC | Content in the second column
Operation ID| Content from cell 2
Plan ID | Content in the second column
Seat Quantity| Content from cell 2
Subscription ID | Content in the second column
Subscription Name | Content in the second column
Is Test Subscription? | Content in the second column
Is Test Subscription? | Content in the second column
Is Test Subscription? | Content in the second column


# Testing Mode

# Integration

Mona SaaS exposes  subscription-related events to your SaaS application through an Event Grid topic. Mona provides the flexibility for integrating multiple options for handling of the subscription-related events. A subscription tells Event Grid which events on a topic you're interested in receiving. When creating the subscription, you provide an endpoint for handling these event. This section serves as a guidline to help with integrating events handlers in Mona. Outlines below ae example event handlers that can be used to integrate events. Please see the following link for more information on other services that can be used to integrate with Event Grid Topics; [Event Handlers](https://docs.microsoft.com/en-us/azure/event-grid/overview#event-handlers).

Please see the below image for a screenshot of an Subscription Event Grid Topic in Mona
(insert img)

## Event Handlers
An [event handler](https://docs.microsoft.com/en-us/azure/event-grid/concepts#event-handlers) is the place where the events are sent. The handler takes an action to process the event. Listed below are a few Azure services that are automatically configured to handle events with the Event Grid. Before integrating please review the supported event handlers for [Azure Event Grid](https://docs.microsoft.com/en-us/azure/event-grid/event-handlers).

#### Azure Functions 
Serverless solution for processing events with minmal code required. For more information, please review the following [docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview).

The following are approaches to using a function as an event handler for [Event Grid Trigger](https://docs.microsoft.com/en-us/azure/event-grid/handler-functions)


#### Logic Apps
Service that helps with automating and orchestrating tasks, business processes and workflows. For more information, please review the following [docs](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-overview). Currenlty Mona has Logic Apps preconfigured out of the box. Please see the below image for reference of Logic App integration in Mona.



Mona Logic App
(Insert Image)



#### Other Event Grid Subscribers 

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
2. Under the Resource folder you will find `.resx` resource files for each view that will contain the translated strings. All views allow to translated the content. You will able to find the `@Localizer[]` tag and the content inside, that will be the content to be translated.

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
