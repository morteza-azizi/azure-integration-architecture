# Competing Consumers

Outbound notifications sitting in a queue. Sending one is slow. Several consumer processes pull from the same `notifications` queue and send in parallel.

```text
Producer
   |
   v
notifications
   |
   +---- consumer-{pid}  (sends notification-1)
   |
   +---- consumer-{pid}  (sends notification-2)
   |
   +---- consumer-{pid}  (sends notification-3)
```

One Producer. One Consumer app. Run the consumer as many times as you want.

## Run

Needs .NET 10 and a Service Bus connection string (emulator or Azure).

Set `ServiceBusConnection` in:

- `src/Producer/appsettings.json`
- `src/Consumer/appsettings.json`

Each consumer names itself `consumer-{pid}` so two launches stay distinct.

```bash
dotnet run --project src/Producer

# as many terminals as you want:
dotnet run --project src/Consumer
```

Each consumer prints the notification it is sending. The same notification is completed by only one of them.

## Test

Starts the Service Bus Emulator, sends 20 messages, runs two consumers, checks both got work.

```bash
dotnet test CompetingConsumers.slnx
```

## Azure

```bash
az group create -n rg-competing-consumers -l westeurope
az deployment group create -g rg-competing-consumers -f infra/main.bicep
```

Then put the namespace connection string in both `appsettings.json` files.

## Article

[article.md](./article.md)

## References

- [EIP — Competing Consumers](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html)
- [Microsoft — Competing Consumers](https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers)
