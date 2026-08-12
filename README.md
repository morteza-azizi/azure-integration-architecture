# Azure Integration Architecture

Engineering home for my Azure Integration Architecture work and article series — practical samples, experiments, and trade-offs around Azure messaging, APIs, and event-driven systems.

## Topics

- Azure Integration Services
- Azure Service Bus
- Messaging
- Event-driven architecture
- APIs and integration
- Resilience
- Integration testing
- Distributed systems

## Existing Work

Published work under [`testing/`](./testing/).

### Azure Service Bus

Local integration testing of Azure Functions with Service Bus Emulator and Testcontainers.

- Sample: [`testing/servicebus-testcontainers/`](./testing/servicebus-testcontainers/)
- [Read the article](https://www.mortezaazizi.com/posts/azure-service-bus-testing-without-the-drama/)

### Azure Storage Queue

Local integration testing of Azure Functions with Azurite and Testcontainers.

- Sample: [`testing/storagequeue-testcontainers/`](./testing/storagequeue-testcontainers/)
- [Read the article](https://www.mortezaazizi.com/posts/azure-storage-queue-testing-journey/)

### Shared testing infrastructure

Shared models and helpers used by the emulator samples:

[`testing/Shared.EmulatorSample/`](./testing/Shared.EmulatorSample/)

## Approach

> Think → Decide → Build → Test → Break → Measure → Explain → Share

Focus is on working implementations, experiments, and the architectural trade-offs that show up when you actually build and test the system.

## What's Next

Future work (not in this repo yet):

- Competing Consumers
- Idempotency
- Inbox Pattern
- Retries
- Bulkhead
- Claim Check
- Wire Tap

## Related

- [Architecture Through Engineering](https://www.mortezaazizi.com/posts/architecture-through-engineering-00-manifesto/) — patterns and distributed-systems concepts explored through building
- [Technical blog](https://www.mortezaazizi.com/)

This repository is Azure-focused: integration, messaging, APIs, eventing, resilience, and practical architecture.

Architecture Through Engineering is separate — deeper exploration of architecture patterns and distributed systems.
