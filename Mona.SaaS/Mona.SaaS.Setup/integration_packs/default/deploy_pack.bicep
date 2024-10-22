param deploymentName string

param location string = resourceGroup().location

param eventGridConnectionName string
param eventGridTopicName string
param managedIdId string

var packName = 'default'

module onPurchased './on_purchased_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-purchased-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}

module onCanceled './on_canceled_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-canceled-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}

module onPlanChanged './on_plan_changed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-plan-changed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}

module onSeatQtyChanged './on_seat_qty_changed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-seat-qty-changed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}

module onReinstated './on_reinstated_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-reinstated-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}

module onSuspended './on_suspended_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-suspended-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}

module onRenewed './on_renewed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-renewed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    eventGridConnectionName: eventGridConnectionName
    eventGridTopicName: eventGridTopicName
    managedIdId: managedIdId
  }
}
