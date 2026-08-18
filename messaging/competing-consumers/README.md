# Competing Consumers

Outbound notifications waiting in a queue. Sending one is slow. Several instances of the same consumer pull from one Azure Service Bus queue and call a downstream API.

This sample makes the Competing Consumers pattern tangible: messages are shared by competition, not by a guaranteed round-robin split. A pattern gives you a mechanism, not an architecture.

## Topology

```text
                  ┌── consumer-{pid} ──┐
                  │                    │
Producer → notifications ── consumer-{pid} ──┼──→ Downstream API
                  │                    │
                  └── consumer-{pid} ──┘
```

This is a **queue**, not a topic. Consumers compete for the same work; they do not each get a copy.

## Parts

- **Producer** — sends `notification-1` … `notification-N` (default 20) to the `notifications` queue.
- **Azure Service Bus queue** — `notifications`. One queue, many consumer processes.
- **Consumer** — one app, run as many times as you want. Each process names itself `consumer-{pid}`. It receives a message, POSTs it to the downstream API, then completes the Service Bus message.
- **Downstream API** — `POST /notifications`. Configurable delay (`Downstream:DelayMs`) and concurrency cap (`Downstream:MaxConcurrentRequests`). No database or business logic; it is a controllable dependency.

## Run

Needs .NET 10 and a Service Bus connection string (emulator or Azure) in:

- `src/Producer/appsettings.json`
- `src/Consumer/appsettings.json`

Azure deploy and knobs: [RUNBOOK.md](./RUNBOOK.md).

From `messaging/competing-consumers`, in separate terminals:

```bash
dotnet run --project src/DownstreamApi
dotnet run --project src/Consumer
dotnet run --project src/Producer
```

Start `src/Consumer` again in more terminals to add competing instances. Ctrl+C stops one process. Logs show `processing` then `completed` for each notification.

## Experiments

Run by hand. There is no benchmark harness. The article walks through:

- **Multiple consumers** — start the same Consumer twice (or more). Work is shared. Do not assume round-robin or an even split.
- **Uneven processing speed** — a faster instance can take more work while a slower one is still busy. The sample’s controllable delay is `Downstream:DelayMs` (shared by every caller).
- **Consumer failure and redelivery** — stop a consumer between `processing` and `completed`. The Service Bus message is completed only after the downstream call succeeds, so it can be delivered again.
- **Ordering** — messages are sent in sequence (`notification-1`, `notification-2`, …). Concurrent consumers do not make in-order completion a guarantee.
- **Downstream API** — more consumers mean more concurrent calls to the same dependency. Delay and `MaxConcurrentRequests` let you feel that pressure; they are not a measured saturation study.

## Lesson

Competing Consumers lets independent work be processed concurrently. It does not tell you how many consumers to run, whether ordering matters, how to handle duplicates, or how much load the downstream system can take. Those are architectural decisions.

## Article

[How Many Consumers Do We Actually Need?](https://www.mortezaazizi.com/posts/competing-consumers-how-many-do-we-actually-need/)

## References

- [EIP — Competing Consumers](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html)
- [Microsoft — Competing Consumers](https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers.html)
