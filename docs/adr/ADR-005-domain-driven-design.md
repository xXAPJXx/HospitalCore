# ADR-005: Domain-Driven Design tactical patterns

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

Hospital software is dominated by business rules: who can be registered, which data is mandatory, which identifiers must be unique. If these rules are scattered across controllers, validators and stored procedures, they drift apart and are hard to test. The Patients module is the first place where these rules appear.

## Problem

Where and how should business rules be modelled so that they are always enforced and easy to test?

## Alternatives considered

1. **Anemic domain model with services and DTO validation** (DataAnnotations or FluentValidation on requests): quick to write, but an entity can be created in an invalid state from anywhere, and rules get duplicated across entry points.
2. **Transaction scripts**: one procedure per operation. Simple for small systems, but logic is repeated as the number of operations grows.
3. **Rich domain model with DDD tactical patterns**: entities protect their own invariants, and value objects make illegal states unrepresentable.

## Decision

- **Aggregates** expose factory methods and keep constructors private. `Patient.Register(...)` is the only way to create a patient, and it validates the date of birth (not in the future, at most 150 years ago) and the administrative sex.
- **Value objects** are immutable records that validate and normalise on creation:
  - `PersonName` collapses whitespace and enforces required, length-limited given and family names.
  - `IdentityDocument` removes separators, upper-cases the number and allows only ASCII letters and digits.
- **Strongly typed identifiers** (`PatientId`) wrap GUID version 7 values. GUIDs can be generated without a database round trip, and version 7 values are time-ordered, which limits index fragmentation in SQL Server.
- **Result pattern for expected failures.** Domain and application operations return `Result` or `Result<T>` with an `Error` that has a stable code (for example, `Patients.DocumentAlreadyRegistered`) and a type (`Validation`, `NotFound`, `Conflict`). The API maps error types to HTTP status codes and exposes the code in ProblemDetails. Exceptions are reserved for programming errors and infrastructure failures.
- **Time is injected.** Handlers obtain the current time from `TimeProvider` and pass it to the domain, so rules that depend on "today" are deterministic in tests.
- **Invariants that need persisted data**, such as identity document uniqueness, are checked by the application handler and guaranteed by a unique database index.
- **Domain events are deferred** until there is a consumer, for example when the Appointments module needs to react to patient registration.
- The **ubiquitous language is English** and borrows concepts from public standards such as HL7 FHIR (for example, the AdministrativeGender value set) without copying any proprietary model.

## Advantages

- An invalid `Patient` cannot exist in memory, so every entry point benefits from the same rules.
- Domain unit tests run in milliseconds with no infrastructure.
- Error codes form a stable contract that clients can rely on.

## Disadvantages

- More code than a CRUD approach: factories, value objects and result handling.
- EF Core needs private constructors and mapping configuration to materialise aggregates.
- Result handling adds explicit checks in handlers.

## Consequences

- New modules follow the same structure: aggregate, value objects, an errors class and a result-returning factory.
- Patient details are not editable yet. Future changes will be expressed as intention-revealing methods (for example, `CorrectName`) rather than public setters.
