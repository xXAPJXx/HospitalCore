# Architecture overview

This document describes the current and target architecture of HospitalCore. Decisions and their trade-offs are recorded in the [Architecture Decision Records](../adr/README.md).

## Architectural style

HospitalCore is a **modular monolith** ([ADR-001](../adr/ADR-001-modular-monolith.md)):

- One deployable application, `HospitalCore.Api`, hosts every module.
- Each business capability lives in its own module under `src/Modules/<Module>`.
- Modules do not reference each other's internals. Cross-module communication will go through explicit contracts, defined in a future ADR when the second module is introduced.
- Each module will own its data through a dedicated database schema.

Inside each module the code follows **Clean Architecture** ([ADR-002](../adr/ADR-002-clean-architecture.md)).

## System context

```mermaid
flowchart LR
    Staff(["Hospital staff"])
    Client["Client application<br/>(future web front end)"]
    Api["HospitalCore.Api"]
    Sql[("SQL Server 2025<br/>schema per module")]
    Redis[("Redis<br/>planned")]

    Staff --> Client --> Api
    Api --> Sql
    Api --> Redis
```

## Project dependencies

Arrows point from a project to the project it depends on.

```mermaid
flowchart TB
    Api["HospitalCore.Api"]

    subgraph Patients["Modules / Patients"]
        PI["Patients.Infrastructure"]
        PA["Patients.Application"]
        PD["Patients.Domain"]
    end

    SK["SharedKernel"]

    Api --> PI
    PI --> PA
    PA --> PD
    PD --> SK

    AT["ArchitectureTests"] -.-> Api
    IT["Api.IntegrationTests"] -.-> Api
```

| Layer | Responsibility | May depend on |
| --- | --- | --- |
| Domain | Entities, value objects, domain events and business invariants | SharedKernel only |
| Application | Use cases (commands and queries), DTOs, ports implemented by Infrastructure | Domain |
| Infrastructure | Persistence with EF Core, external services, module registration | Application, Domain |
| Api | HTTP endpoints, middleware, configuration, dependency injection composition | Module Infrastructure and Application |

These rules are enforced by `tests/HospitalCore.ArchitectureTests`.

## Request flow

This is how `POST /api/patients` works today. Query handlers follow the same path without the domain operation and the save.

```mermaid
sequenceDiagram
    participant C as Client
    participant Ctl as Controller (Api)
    participant H as Command handler (Application)
    participant D as Aggregate (Domain)
    participant R as Repository (Infrastructure)
    participant DB as SQL Server

    C->>Ctl: POST /api/patients
    Ctl->>H: RegisterPatientCommand
    H->>D: Create value objects and Patient.Register(...)
    D-->>H: Result (patient or validation error)
    H->>R: Is the identity document already registered?
    R->>DB: SELECT ... WHERE DocumentType = @t AND DocumentNumber = @n
    H->>R: Add patient and save changes
    R->>DB: INSERT INTO patients.Patients
    H-->>Ctl: Result<Guid>
    Ctl-->>C: 201 Created, or ProblemDetails (400 / 409)
```

## Cross-cutting concerns

| Concern | Current state |
| --- | --- |
| Error handling | Expected failures travel as `Result` values with stable error codes and are mapped to RFC 9457 ProblemDetails (`400`, `404`, `409`) with a `code` extension. Unexpected exceptions are handled by `UseExceptionHandler` |
| Persistence | EF Core 10 with one `DbContext` and one SQL Server schema per module; migrations applied explicitly with `dotnet ef` |
| Time | `TimeProvider` is injected so date rules are deterministic in tests |
| Health | `GET /health` liveness endpoint |
| API documentation | Built-in OpenAPI document generation with Swagger UI in Development |
| Logging | Default ASP.NET Core logging; structured logging provider to be decided |
| Security | Not implemented yet; see future ADR-006 |
