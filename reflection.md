# Reflection

## AI Hallucinations Encountered

- Hallucination 1

```
Copilot initially generated route parameters.

Requirement required query parameters.

Corrected manually.
```

- Hallucination 2

```
Copilot suggested direct provider injection.

Refactored to interface-based dependency injection.
```

- Hallucination 3

```
Timestamp selection logic required manual verification.

Added tests to validate latest provider wins.
```
- Hallucination 4

```

CORS issue encountered during Angular integration.
Resolved by configuring explicit CORS policy.

## Prompt Refinements

- Tighter constraints on provider timestamps and error handling.
- Explicit guidance to use xUnit and .NET 8.

## Validation Strategy

- Unit tests for aggregation service cover most recent selection, failure tolerance, and mapping.
- Manual smoke tests run the API and frontend locally.

## Future Improvements

- Add contract tests for provider responses.
- Add automated end-to-end tests for API + UI.
