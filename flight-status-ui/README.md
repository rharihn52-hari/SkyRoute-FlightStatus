# SkyRoute Flight Status

## Overview

SkyRoute Flight Status is a full-stack flight status aggregation application built using .NET 8 Minimal APIs and Angular.

The application queries multiple flight status providers (AeroTrack and QuickFlight), normalizes provider-specific responses into a unified model, and selects the most recently updated result.

## Architecture

### Backend

* .NET 8 Minimal API
* Clean Architecture principles
* Dependency Injection
* Provider abstraction using `IFlightStatusProvider`
* Aggregation service for provider selection
* Swagger/OpenAPI support

### Frontend

* Angular
* Reactive Forms
* HttpClient
* Status-based UI rendering

## Provider Selection Logic

The aggregation service queries all providers in parallel.

Rules:

1. If multiple providers return data, select the result with the latest `LastUpdatedUtc`.
2. If only one provider returns data, use that result.
3. If no providers return data, return `Unknown`.

## Running the Backend

```bash
dotnet run --project src/FlightStatus.Api
```

Swagger:

http://localhost:5000/swagger

## Running the Frontend

```bash
cd flight-status-ui
npm install
npm start
```

UI:

http://localhost:4200

## Running Tests

```bash
dotnet test tests/FlightStatus.Tests/FlightStatus.Tests.csproj
```

## Sample Flight Numbers

| Flight Number | Expected Status |
| ------------- | --------------- |
| AI101         | Delayed         |
| AI202         | OnTime          |
| BA303         | Cancelled       |
| AI404         | Diverted        |
| XYZ999        | Unknown         |

## AI Usage

GitHub Copilot was used for:

* Solution scaffolding
* Test generation
* UI generation
* Documentation assistance

All generated output was reviewed, validated, and tested manually.

## Future Improvements

* Redis caching
* Additional providers
* CI/CD pipeline
* End-to-end testing
