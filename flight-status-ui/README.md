# SkyRoute Flight Status Aggregator

## Overview

SkyRoute Flight Status is a flight status aggregation system that queries multiple upstream providers in parallel, normalizes their responses, and returns the most recently updated result. It demonstrates Clean Architecture, provider abstraction, fault tolerance, and AI-assisted development using GitHub Copilot.

## Architecture

```
Angular 21 (Standalone)
    │
    ▼
ASP.NET Core Minimal API (.NET 8)
    │
    ▼
FlightStatusAggregationService
    │
    ├──► IFlightStatusProvider (AeroTrack)
    │
    └──► IFlightStatusProvider (QuickFlight)
```

**Key design decisions:**

- **Provider Pattern** — Each provider implements `IFlightStatusProvider`, enabling new sources to be added without modifying aggregation logic (Open/Closed Principle).
- **Parallel Execution** — Providers are queried concurrently via `Task.WhenAll` for minimum latency.
- **Latest Wins** — When multiple providers return data, the result with the most recent `LastUpdatedUtc` is selected.
- **Fault Tolerance** — Individual provider failures are caught and logged; the aggregator continues with remaining results.
- **Unknown Fallback** — If no provider returns data, a deterministic `Unknown` status is returned (never null).

## API Contract

```
GET /flights/status?flightNumber={flightNumber}&date={yyyy-MM-dd}
```

**Parameters:**

| Parameter | Type | Required | Example |
|---|---|---|---|
| `flightNumber` | string | Yes | AI101 |
| `date` | string (ISO date) | Yes | 2026-06-14 |

**Success Response (200):**

```json
{
  "flightNumber": "AI101",
  "flightDate": "2026-06-14",
  "status": 2,
  "lastUpdatedUtc": "2026-06-14T09:55:00Z",
  "providerName": "QuickFlight",
  "gate": "A12",
  "terminal": "T1",
  "delayReason": "Weather",
  "message": "Flight delayed due to adverse weather conditions"
}
```

**Status Enum:**

| Value | Meaning |
|---|---|
| 0 | Unknown |
| 1 | OnTime |
| 2 | Delayed |
| 3 | Cancelled |
| 4 | Diverted |

**Error Response (400):** Returned when `flightNumber` or `date` is missing or invalid.

## Sample Flight Numbers

| Flight | Expected Status | Winning Provider | Gate | Terminal | Delay Reason |
|---|---|---|---|---|---|
| AI101 | Delayed | QuickFlight | A12 | T1 | Weather |
| AI202 | OnTime | AeroTrack | — | — | — |
| BA303 | Cancelled | QuickFlight | — | — | — |
| AI404 | Diverted | AeroTrack | — | — | — |
| XYZ999 | Unknown | None | — | — | — |

## Project Structure

```
├── src/
│   ├── FlightStatus.Api/              # Minimal API, DI, CORS, Swagger
│   ├── FlightStatus.Application/      # Aggregation service, interfaces
│   ├── FlightStatus.Domain/           # FlightStatusResult, FlightStatus enum
│   └── FlightStatus.Infrastructure/   # AeroTrack, QuickFlight providers
├── tests/
│   └── FlightStatus.Tests/            # xUnit tests (aggregation, mapping, failures)
├── flight-status-ui/                  # Angular 21 standalone frontend
├── spec.md                            # Original assignment specification
├── prompts.md                         # AI prompt log with governance trail
└── reflection.md                      # AI hallucinations and lessons learned
```

## How To Run

### Backend

```bash
dotnet run --project src/FlightStatus.Api
```

API starts at `http://localhost:5000`. Swagger UI available at `/swagger`.

### Frontend

```bash
cd flight-status-ui
npm install
ng serve
```

Open `http://localhost:4200`. The UI calls the backend API on port 5000.

### Tests

```bash
dotnet test
```

Runs xUnit tests covering: latest-timestamp selection, unknown fallback, provider failure tolerance, and per-provider status mapping.

## AI Tooling

- **GitHub Copilot Agent Mode** — Scaffolding, provider generation, test generation.
- **GitHub Copilot Chat** — Architecture decisions, debugging, documentation.
- All generated code was manually reviewed, compiled, and validated through unit tests and manual Swagger/UI testing.
- See `prompts.md` for the full prompt-by-prompt log and `reflection.md` for hallucinations encountered.

## Production Considerations

If this were a production system, the following would be added:

- **Caching** — Redis cache layer to reduce provider call frequency.
- **Resilience** — Polly retry/circuit-breaker policies on provider HTTP calls.
- **Observability** — OpenTelemetry tracing and structured logging.
- **Health Checks** — `/health` endpoint for orchestrator liveness probes.
- **CI/CD** — GitHub Actions pipeline for build, test, and deployment.