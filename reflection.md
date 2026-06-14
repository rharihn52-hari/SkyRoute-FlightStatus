# Reflection

## AI Hallucinations Encountered

### Hallucination 1 — Route Parameters vs Query Parameters

```
Copilot initially generated route parameters for the flight status endpoint.
The requirement specified query parameters (?flightNumber=...&date=...).
Corrected manually to match the API contract.
```

**Impact:** Would have caused 404 errors from the frontend.
**Detection:** Manual review during API integration testing.

### Hallucination 2 — Direct Provider Injection

```
Copilot suggested injecting concrete provider classes directly into the aggregation service.
Refactored to interface-based dependency injection using IFlightStatusProvider.
```

**Impact:** Would have violated Open/Closed Principle and made testing difficult.
**Detection:** Architecture review against Clean Architecture guidelines.

### Hallucination 3 — Timestamp Selection Logic

```
Copilot's initial aggregation selected the first non-null result rather than the most recent.
Timestamp selection logic required manual verification and correction.
Added unit tests to validate latest provider wins.
```

**Impact:** Would have returned stale provider data instead of the freshest result.
**Detection:** Unit test `LatestTimestamp_ShouldWin` exposed the issue.

### Hallucination 4 — CORS Policy Missing

```
Copilot generated the Angular frontend service pointing to localhost:5000,
but did not add CORS configuration to the .NET API.
Browser blocked all cross-origin requests between localhost:4200 and localhost:5000.
Resolved by adding explicit CORS policy in Program.cs.
```

**Impact:** Frontend could not communicate with backend at all during local development.
**Detection:** Browser console showed `Access-Control-Allow-Origin` errors on first integration test.
**Resolution:** Added `builder.Services.AddCors()` with `AllowAnyOrigin` policy. Production would require explicit origin whitelisting.

## Prompt Refinements

- Tighter constraints on provider timestamps and error handling after Hallucination 3.
- Explicit guidance to use xUnit and .NET 8 after Copilot defaulted to NUnit in early iterations.
- Added query-parameter format to prompt context after the route-parameter hallucination.

## Validation Strategy

- **Unit tests** — Aggregation service tests cover most recent selection, failure tolerance, unknown fallback, and per-provider mapping.
- **Swagger testing** — Verified API responses directly through Swagger UI before wiring frontend.
- **Manual smoke tests** — Ran all five sample flights (AI101, AI202, BA303, AI404, XYZ999) through the Angular UI and verified status, provider, and detail fields.
- **Code review** — Every Copilot-generated file was read, understood, and modified where needed before committing.

## Future Improvements

- Add contract tests for provider responses to catch schema drift.
- Add automated end-to-end tests for API + UI using Playwright.
- Add integration tests that spin up the API in-memory using `WebApplicationFactory`.