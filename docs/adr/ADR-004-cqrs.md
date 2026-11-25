# ADR-004: CQRS without a mediator library

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

Hospital use cases fall into two very different groups. Commands such as registering a patient change state and must enforce business rules. Queries such as viewing a patient only read data and should be simple and fast. Organising code by use case keeps each change small and easy to review.

## Problem

How should application use cases be structured, should reads and writes be separated, and how should HTTP endpoints invoke them?

## Alternatives considered

1. **Service classes** (for example, `PatientService` with many methods): familiar, but services grow without limit and mix unrelated dependencies.
2. **Full CQRS** with separate read and write databases or event sourcing: powerful for high-scale systems, but far beyond the current needs and expensive to operate.
3. **CQRS-lite**: one handler per use case, with commands and queries separated in code but sharing the same database. Handlers can be dispatched in several ways:
   - **MediatR**: widely used, but since 2025 new versions are distributed under a commercial license, with a free community tier subject to conditions, and it adds indirection that hides the call path.
   - **Source-generated mediator libraries**: fast and free, but still an extra dependency and concept.
   - **Dependency injection of handler interfaces**: explicit, no dependency, easy to navigate with "Go to implementation".

## Decision

- Use **CQRS-lite**. Each use case has a request type and a handler implementing `ICommandHandler<TCommand, TResponse>` or `IQueryHandler<TQuery, TResponse>` from `HospitalCore.SharedKernel.Messaging`.
- **Commands** go through the domain model and persist through repositories and a unit of work.
- **Queries** return DTOs, never domain entities. They currently read through the repository and may switch to dedicated projections when read needs diverge.
- **No mediator library.** Controllers receive the handler they need through `[FromServices]` and call it directly.
- Cross-cutting behaviour such as logging, validation or transactions will be added as decorators around the handler interfaces when it is needed.
- Use case folders inside the Application project (`RegisterPatient/`, `GetPatientById/`) keep each slice cohesive.

## Advantages

- The call path from controller to handler is explicit and debuggable.
- Each handler has only the dependencies its use case needs, which keeps unit tests small.
- No licensing or version risk from a third-party mediator.

## Disadvantages

- Handlers are registered one by one in `PatientsModule`. This can be automated with assembly scanning when the number grows.
- Decorators require more manual wiring than mediator pipeline behaviours.
- The messaging interfaces live in the shared kernel, which the domain projects also reference. If the shared kernel grows, the application-level abstractions should move to a separate building block.

## Consequences

- Every new use case adds one request type, one handler, one registration and its tests.
- Read models can evolve independently (projections, caching with Redis) without touching command handlers.
