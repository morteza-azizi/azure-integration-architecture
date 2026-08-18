# Runbook

Needs a Service Bus connection string in:

- `src/Producer/appsettings.json`
- `src/Consumer/appsettings.json`

Optional Azure deploy:

```bash
az group create -n rg-competing-consumers -l westeurope
az deployment group create -g rg-competing-consumers -f infra/main.bicep
```

Copy `serviceBusConnectionString` from the output into both files.

## Run

From `messaging/competing-consumers`, in separate terminals:

```bash
dotnet run --project src/DownstreamApi
dotnet run --project src/Consumer
dotnet run --project src/Producer
```

Start as many consumers as you want. Ctrl+C stops one cleanly.

## Knobs

`src/DownstreamApi/appsettings.json`:

- `Downstream:DelayMs` — how long each send takes (`0`, `200`, `2000`)
- `Downstream:MaxConcurrentRequests` — how many sends at once (`1`, `2`, `8`)

Restart the API after changing them.
