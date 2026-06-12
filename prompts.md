# AI Prompt Log

## Prompt 1

Purpose:
Generate repository-level Copilot instructions and establish development standards.

Outcome:
Created `.github/copilot-instructions.md` with architectural guidelines.

Changes:
Defined standards for .NET 8, Clean Architecture, dependency injection, async/await patterns, testing with xUnit, DTOs, logging, error handling, and SOLID principles.

Reason:
Needed explicit operational requirements and architectural guidance for consistency across codebase.

---

## Prompt 2

Purpose:
Generate domain model and provider abstraction for flight status functionality.

Accepted:
Most of the generated structure for FlightStatus enum, FlightStatusResult record, and IFlightStatusProvider interface.

Modified:
Added Message field to FlightStatusResult and included XML documentation comments for public members.

Reason:
Needed support for Unknown responses, provider-specific details, and API documentation clarity.

---

## Prompt 3

Purpose:
Implement stub providers (AeroTrack and QuickFlight) and flight status aggregation service.

Accepted:
Provider structure with deterministic hardcoded responses and aggregation logic using Task.WhenAll.

Modified:
Added comprehensive error handling in aggregation service to catch and log provider failures without failing the request.

Reason:
Challenge requires deterministic, testable behaviour and resilience to provider failures - core requirements from the PDF.

---

## Prompt 4

Purpose:
Create backend unit tests that prove aggregation selection, null handling, provider failure resilience, and provider status mapping.

Accepted:
Test scenarios for most recent provider selection, unknown fallback, provider exception recovery, and provider mapping coverage.

Modified:
Added deterministic provider cases for additional status mappings and aligned the test project with .NET 8.

Reason:
Backend scoring is highest for tests right now, so I focused on the exact PDF scenarios before moving to UI.

---

## Prompt 4

Purpose:
Create RESTful API endpoint with dependency injection configuration and input validation.

Accepted:
API endpoint structure with query parameters, Swagger integration, and DI registration.

Modified:
Added comprehensive input validation for null/empty flight numbers and dates, proper error responses with 400 status codes, logging at key points, and namespace aliasing to resolve enum name collisions.

Reason:
PDF explicitly requires GET /flights/status endpoint with query parameters and 400 validation errors. Tested endpoint successfully - verified aggregation service selects most recent provider (QuickFlight -5min over AeroTrack -10min).

---
