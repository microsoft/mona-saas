# Contents
* [Introduction](#introduction)
* [Events](#events)
* [Testing Mode](#testing-mode)
* [Integration](#configuration) 
* [Language Support](#language-support)
* [Conclusion](#conclusion)
# Introduction

Welcome to the Mona SaaS developer guide. This document serves as a guide


# Events

Each of these operations is exposed to your SaaS application by Mona SaaS through events published to a custom Event Grid topic automatically provisioned during setup. By default, we deploy a set of "stub" Logic Apps into your Azure subscription that are enabled by default and configured to be triggered by these subscription events.

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

This is the base subscription event when 

```json
{
  "id": "<guid>", 
  "subscriptionName": "Contoso Cloud Solution", 
  "offerId": "offer1",
  "planId": "silver", 
  "quantity": "20", 
  "subscription": { 
    "id": "<guid>",
    "publisherId": "contoso",
    "offerId": "offer1",
    "name": "Contoso Cloud Solution",
    "saasSubscriptionStatus": " PendingFulfillmentStart ",
    "beneficiary": {
      "emailId": "test@test.com",
      "objectId": "<guid>",
      "tenantId": "<guid>",
      "pid": "<ID of the user>"
    }
```
#### SubscriptionCancelled
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

#### SubscriptionPlanChanged
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
```
#### SubscriptionReinstated
```json
```
### SubscriptionSeatQuantityChanged
```json
```
#### SubscriptionSuspended
```json
```

First Header | Second Header
------------ | -------------
Content from cell 1 | Content from cell 2
Content in the first column | Content in the second column

# Testing Mode

# Integration guidance

# Language Support

# Conclusion
