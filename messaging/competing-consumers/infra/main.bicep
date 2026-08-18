param namePrefix string = 'cc'
param location string = resourceGroup().location

var namespaceName = '${namePrefix}-sb-${uniqueString(resourceGroup().id)}'

resource namespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource queue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'notifications'
  properties: {
    requiresSession: false
    maxDeliveryCount: 10
    lockDuration: 'PT1M'
  }
}

resource defaultAuth 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' existing = {
  parent: namespace
  name: 'RootManageSharedAccessKey'
}

output serviceBusFullyQualifiedNamespace string = '${namespace.name}.servicebus.windows.net'
output queueName string = queue.name
output serviceBusConnectionString string = defaultAuth.listKeys().primaryConnectionString
