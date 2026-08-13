# How Many Consumers Do We Actually Need?

> Exploring the Competing Consumers pattern with Azure Service Bus

## The problem

A producer is queuing outbound notifications. Each send takes time. One consumer cannot keep up. The `notifications` queue grows.

The question becomes: do we need more consumers?

## The known pattern

Yes. Multiple instances pull from the **same** queue and compete for the next message.

This is not new. It is documented in:

- [Enterprise Integration Patterns — Competing Consumers](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html)
- [Microsoft Azure Architecture Center — Competing Consumers](https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers)

Read those for the definition. This folder is a small implementation you can run.

## What we built

```text
Producer → notifications → consumer-{pid}
                           → consumer-02
                           → consumer-N
```

- One Producer console app
- One Consumer console app (`ServiceBusProcessor`)
- Instance count is just how many times you start the consumer
- Local tests use the Service Bus Emulator via Testcontainers
- Azure: one Bicep file for a namespace + queue

No shared library. No metrics platform. No extra apps.

## Try it

```bash
# set ServiceBusConnection in src/Producer/appsettings.json and src/Consumer/appsettings.json
dotnet run --project src/Producer
dotnet run --project src/Consumer
```

Watch the logs. Each `notification-N` is sent once. Different consumers take different notifications.

## What to notice

- Adding another process is enough. You do not need a second application.
- A queue (not a topic) is what makes them compete.
- Completion order is not send order. Sessions would be a different experiment.
- If a consumer dies mid-message, Service Bus redelivers after the lock expires. That is why the next article is **Idempotency**.

## Next: Idempotency

We've solved one problem: one consumer wasn't enough.

The next problem:

> What happens when the same message is processed twice?
