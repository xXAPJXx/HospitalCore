# ADR-002: Clean Architecture inside each module

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

[ADR-001](ADR-001-modular-monolith.md) splits HospitalCore into business modules. Each module still needs an internal structure. Hospital business rules, such as admission eligibility, appointment overlaps or billing rules, are the most valuable and long-lived part of the code. Frameworks such as ASP.NET Core and Entity Framework Core will change on their own schedule.

## Problem

How should code inside a module be organised so that business rules are isolated from frameworks, easy to test and easy to locate?

## Alternatives considered

1. **N-tier (presentation, business logic, data access)**: business logic depends directly on data access.
   - Pros: familiar and quick for CRUD.
   - Cons: business rules become coupled to the ORM and the database. Testing them requires a database or heavy mocking.
2. **Vertical slice architecture**: code is organised by feature, with minimal shared layering.
   - Pros: high cohesion per feature, little ceremony.
   - Cons: without discipline, domain rules get duplicated across slices. There is no explicit place for a rich domain model.
3. **Clean Architecture**: concentric layers where dependencies point inwards, towards the domain.
   - Pros: framework-independent domain, high testability, clear placement rules that interviewers and new contributors recognise.
   - Cons: more projects and some indirection.

## Decision

Each module uses **Clean Architecture** with three projects:

| Project | Contains | Depends on |
| --- | --- | --- |
| `Domain` | Entities, aggregates, value objects, domain events, invariants | `SharedKernel` |
| `Application` | Use cases as commands and queries, DTOs, interfaces (ports) for persistence and external services | `Domain` |
| `Infrastructure` | EF Core persistence, implementations of Application interfaces, module dependency injection registration | `Application` |

The API host contains thin controllers that translate HTTP requests into application commands and queries. Inside the Application project, use cases are grouped by feature (one folder per use case), which keeps the cohesion benefit of vertical slices.

The dependency rule is enforced automatically by `tests/HospitalCore.ArchitectureTests` using NetArchTest.

## Advantages

- Business rules can be unit tested without a database, web server or mocks of framework types.
- Infrastructure choices, such as the database provider or caching, can change without touching the domain.
- Every type has an obvious home, which reduces design discussions and review effort.

## Disadvantages

- Three projects per module plus the host add build time and navigation overhead.
- Interfaces between Application and Infrastructure add indirection.
- For simple reference data (for example, Administration catalogues) the full structure can be heavier than necessary.

## Consequences

- Controllers must not contain business logic and must not expose domain entities. They return DTOs.
- The Domain project must never reference Entity Framework Core or ASP.NET Core. Persistence mapping lives in Infrastructure.
- Modules that turn out to be simple CRUD may adopt a lighter structure, but only through a new ADR.
