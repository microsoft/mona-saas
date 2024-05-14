param deploymentName string

param location string = resourceGroup().location

param externalMidId string
param internalMidId string

// For subscribing to the event grid topic...

param eventGridConnectionName string = 'azureeventgrid'
param eventGridTopicName string = 'mona-events-${deploymentName}'

var packName = 'default'

module onPurchased './on_purchased_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-purchased-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}

module onCanceled './on_canceled_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-canceled-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}

module onPlanChanged './on_plan_changed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-plan-changed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}

module onSeatQtyChanged './on_seat_qty_changed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-seat-qty-changed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}

module onReinstated './on_reinstated_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-reinstated-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}

module onSuspended './on_suspended_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-suspended-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}

module onRenewed './on_renewed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-renewed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    externalMidId: externalMidId
    internalMidId: internalMidId
  }
}
