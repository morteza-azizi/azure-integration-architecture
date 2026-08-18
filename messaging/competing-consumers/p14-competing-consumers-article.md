# How Many Consumers Do We Actually Need?

> Exploring the Competing Consumers pattern with Azure Service Bus

## The problem

Imagine a queue of notifications waiting to be processed.

A producer keeps adding messages, while one consumer processes them one at a time.

```text
Producer
   |
   v
┌───────────────┐
│ notifications │
└───────┬───────┘
        |
        v
    Consumer
```

If messages arrive faster than processing them, the queue starts to grow.

One obvious question is:

> **Can we simply add more consumers?**

For example:

```text
                 ┌── Consumer 1
                 │
Producer → Queue ├── Consumer 2
                 │
                 └── Consumer 3
```

This is the **Competing Consumers** pattern.

It sounds simple. And the basic idea is simple. But once we start running it, a few interesting questions appear:

- How are messages actually distributed?
- Does adding consumers always make things faster?
- What happens if one consumer is slower?
- What happens if a consumer disappears while processing a message?
- What happens to ordering?
- When does adding more consumers stop helping?

Those are the questions I want to explore.

---

## The pattern is not new

Let's get this out of the way first.

Competing Consumers is not a new idea.

It is described in [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html), and Microsoft has documented the pattern in the [Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers).

Both are excellent references for understanding the pattern itself.

So I'm not trying to explain something new here.

**Instead, I want to take the concept and make it tangible.**

I'll build a small .NET implementation using **Azure Service Bus**, deploy the infrastructure with **Bicep**, run multiple consumers, and see how the pattern actually behaves.

> **Build it, run it, change it, and see what actually happens.**

Reading about a distributed-systems pattern and watching it behave are two different things.

---

## A deliberately small implementation

I wanted the implementation to be boring.

No messaging framework.

No benchmark platform.

No metrics infrastructure.

No extra services.

Just:

```text
Producer
   |
   v
Azure Service Bus Queue
   |
   +── Consumer
   +── Consumer
   +── Consumer
```

There is only **one Consumer application**.

The interesting part is that I can start the same application multiple times.

Each process connects to the same `notifications` queue.

So I don't need:

```text
ConsumerA
ConsumerB
ConsumerC
```

I simply run:

```text
Consumer
Consumer
Consumer
```

The code stays the same. The only thing that changes is how many instances are running against the same queue.
---

## The Producer

The producer does very little.

It connects to Azure Service Bus, creates a sender, and sends notification messages to the queue.

The message itself is intentionally simple.

```text
notification-1
notification-2
notification-3
...
```

There is no business domain hiding the idea. The goal is to make the message flow obvious.

---

## The Consumer

The consumer is equally small. It creates a `ServiceBusProcessor` for the queue and registers handlers for messages.

Conceptually:

```text
Receive message
      ↓
Process message
      ↓
Complete message
```

For the demo, processing is simulated with a small delay. The consumer also prints its process ID so we can see which instance handled each message.

This is useful when we start multiple instances:

```text
consumer-1234 processed notification-1
consumer-5678 processed notification-2
```

This gives us a simple way to see which consumer instance processed each message.

---

## Start with one consumer

Before adding anything, start just one.

```text
Producer
   |
   v
Queue
   |
   v
Consumer 1
```

Send 20 messages.

The consumer starts processing the messages from the queue.

Nothing surprising yet.

But this gives us a baseline.

---

## Now add another consumer

Start the same Consumer application again.

We now have:

```text
                 ┌── Consumer 1
                 │
Producer → Queue ├── Consumer 2
                 │
                 └──
```

Send another batch of messages.

Both consumers are now connected to the same queue.

A message is not processed by both consumers simply because both are listening.

Instead, the consumers compete for available messages.

For example, we might see something like:

```text
Consumer 1 → notification-1
Consumer 2 → notification-2
Consumer 1 → notification-3
Consumer 2 → notification-4
```

But we shouldn't expect a perfect alternation.

Consumer 1 doesn't necessarily get every other message, and Consumer 2 doesn't necessarily get the others.

What we're interested in is the bigger observation:

Multiple instances of the same consumer can share the work from a single queue.

And that is the core idea behind Competing Consumers.

---

## Why a queue?

This distinction matters.

We're using:

```text
Producer → Queue → Consumers
```

not:

```text
Producer → Topic → Subscriptions
```

A message is intended to be processed by one consumer instance, rather than being delivered independently to every consumer.

If I need multiple independent subscribers to receive their own copy of a message, that's a different messaging problem.

This is one reason I like building the example instead of only looking at the pattern diagram: the topology makes the intended behaviour much easier to understand.

---

## But does more consumers mean more throughput?

This is where things get more interesting.

It is tempting to conclude:

> One consumer is slow → add consumers → problem solved.

Sometimes that's exactly what we want.

But adding consumers doesn't automatically mean the whole system becomes faster.

The consumers may not be the bottleneck. Something downstream might be.

And some workloads simply cannot be processed safely in parallel.

So instead of assuming that more consumers means more throughput, **let's test it.**

Start with:

```text
1 consumer
2 consumers
4 consumers
```

Keep the workload the same and see what changes.

That's more in line with the approach of this article: **don't assume — run the experiment.**

---

# What I want to investigate next

The basic example works. Now I want to change a few things and see what happens.

### 1. Add consumers

Start with:

```text
1 consumer
2 consumers
4 consumers
```

Keep the workload the same.

What changes?

### 2. Make one consumer slow

What happens if one instance takes much longer to process a message?

```text
Consumer 1 → 200 ms
Consumer 2 → 200 ms
Consumer 3 → 2 seconds
```

Does the slow consumer affect everyone else?

### 3. Stop a consumer

What happens if a consumer disappears while processing a message?

Does the message disappear?

Is it delivered again?

How does Azure Service Bus handle it?

### 4. Look at ordering

Send:

```text
1
2
3
4
5
6
```

with multiple consumers.

Do messages finish in the same order?

### 5. Find the bottleneck

If four consumers are faster than one, what happens with eight?

And what if the downstream system can only handle two concurrent operations?

At some point, adding consumers may stop helping.

**Rather than guessing, let's run the experiments and see what the system actually does.**

---

# One question leads to another

Competing Consumers gives us a way to distribute work across multiple consumer instances.

But distributing work introduces another important question:

> **What happens when the same message is processed twice?**

That's where the next pattern becomes interesting:

**Idempotency.**

---

## References

- [Enterprise Integration Patterns — Competing Consumers](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html)
- [Microsoft Azure Architecture Center — Competing Consumers](https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers)

**Implementation:** `azure-integration-architecture/messaging/competing-consumers/`
