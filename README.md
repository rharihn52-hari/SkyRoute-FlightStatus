# SkyRoute Flight Status

## Overview

SkyRoute Flight Status provides a minimal backend API and Angular frontend to aggregate flight status information from multiple providers and expose a single consolidated response.

## Assumptions

- Providers return time-stamped status entries.
- The most recent provider result is authoritative.
- Providers may fail; the aggregator tolerates provider errors.

## Architecture

This solution follows Clean Architecture principles: a thin API layer, application services for business logic, domain models, and infrastructure providers. Dependency injection is used for provider abstractions.

## Project Structure

- `src/FlightStatus.Api` — ASP.NET Core Web API project.
- `src/FlightStatus.Application` — Application services and interfaces.
- `src/FlightStatus.Domain` — Domain models and enums.
- `src/FlightStatus.Infrastructure` — Provider implementations.
- `flight-status-ui` — Angular frontend.
- `tests/FlightStatus.Tests` — xUnit unit tests for aggregation logic.

## How To Run Backend

From repository root:

```powershell
dotnet run --project src/FlightStatus.Api
```

The API exposes the flight status endpoint used by the UI and tests.

## How To Run Frontend

From repository root:

```powershell
npm install --prefix flight-status-ui
npm start --prefix flight-status-ui
```

Open http://localhost:4200 in your browser after the frontend starts.

## Testing

Run unit tests:

```powershell
dotnet test
```

## Copilot Usage

- Used Copilot Agent Mode for project scaffolding
- Used Copilot Chat for provider generation
- Used Copilot for unit test generation
- Reviewed all generated code manually
- Added tests to validate business logic

## Future Improvements

- Add integration tests that run the API and UI together.
- Add CI pipeline to run tests and linting.
- Improve provider discovery and configuration at runtime.
