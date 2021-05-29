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

# Language Support

# Conclusion
