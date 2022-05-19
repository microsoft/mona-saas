param deploymentName string

param location string = resourceGroup().location

// Used to authenticate webhook callbacks to the Marketplace API

param aadClientId string
param aadTenantId string

@secure()
param aadClientSecret string

var packName = 'default'

module onPurchased './on_purchased_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-purchased-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    aadClientId: aadClientId
    aadClientSecret: aadClientSecret
    aadTenantId: aadTenantId
  }
}

module onCanceled './on_canceled_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-canceled-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
  }
}

module onPlanChanged './on_plan_changed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-plan-changed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    aadClientId: aadClientId
    aadClientSecret: aadClientSecret
    aadTenantId: aadTenantId
  }
}

module onSeatQtyChanged './on_seat_qty_changed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-seat-qty-changed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    aadClientId: aadClientId
    aadClientSecret: aadClientSecret
    aadTenantId: aadTenantId
  }
}

module onReinstated './on_reinstated_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-reinstated-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
    aadClientId: aadClientId
    aadClientSecret: aadClientSecret
    aadTenantId: aadTenantId
  }
}

module onSuspended './on_suspended_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-suspended-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
  }
}

module onRenewed './on_renewed_workflow.bicep' = {
  name: '${packName}-pack-deploy-on-renewed-${deploymentName}'
  params: {
    deploymentName: deploymentName
    location: location
  }
}
