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
