# Experiments

This sample is meant to be run by hand. There is no benchmark harness.

## See competing consumers

```bash
# set ServiceBusConnection in both appsettings.json files
dotnet run --project src/Producer
dotnet run --project src/Consumer
```

Watch which consumer sends which `notification-N`.

Automated check: `dotnet test` (2 consumers, 20 messages, emulator).

## Things you can try

| Try | How |
|-----|-----|
| More consumers | Start `dotnet run --project src/Consumer` again |
| Slow consumer | Increase the `Task.Delay` in `Consumer/Program.cs` on one instance |
| Failure | Kill a consumer while it is processing |
| Ordering | Compare queue order (`notification-1` …) with send order |

Record what you see. Do not invent numbers.
