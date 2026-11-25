# ADR-001: Modular monolith architecture

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

HospitalCore covers several business capabilities with different rules and vocabulary: patients, appointments, admissions, laboratory, clinical records, billing and administration. Some of them, such as admissions and billing, interact closely, while others are largely independent.

The project is developed by a single engineer with a limited weekly time budget. It must still demonstrate architectural judgement that would hold up in a professional team.

## Problem

How should the system be split into deployable units and code boundaries so that business capabilities stay decoupled without paying for operational complexity the project does not need?

## Alternatives considered

1. **Traditional layered monolith**: one set of layers (UI, business, data) shared by the whole application.
   - Pros: simplest to start, familiar.
   - Cons: nothing prevents one capability from using another's internals. Over time the code becomes tightly coupled and hard to change or extract.
2. **Microservices**: one independently deployable service per capability.
   - Pros: independent deployment and scaling, strong isolation, technology freedom per service.
   - Cons: network calls, distributed transactions, eventual consistency, service discovery, distributed tracing and much more infrastructure. That cost is justified by large teams and independent scaling needs, neither of which exists here.
3. **Modular monolith**: a single deployable application made of modules with explicit, enforced boundaries.
   - Pros: the decoupling benefits of services with the operational simplicity of one process.
   - Cons: modules share a runtime and are deployed together.

## Decision

HospitalCore is built as a **modular monolith**:

- `HospitalCore.Api` is the only deployable host and composes all modules.
- Each module lives in `src/Modules/<Module>` and is split into Domain, Application and Infrastructure projects (see [ADR-002](ADR-002-clean-architecture.md)).
- A module must not reference another module's projects. The mechanism for cross-module communication (public contracts, in-process integration events or both) will be decided in its own ADR when the second module is introduced.
- Each module will own its data in a dedicated SQL Server schema within the same database. Modules will not query other modules' tables.
- A module is created when its first feature is implemented. Only the Patients module is scaffolded today.

## Advantages

- One build, one deployment and one process to debug.
- In-process calls: no network latency or partial failures between modules.
- Strong consistency inside a module with ordinary database transactions.
- Clear boundaries make it possible to extract a module into a service later if a real need appears.

## Disadvantages

- All modules share resources: a memory leak or crash in one affects all of them.
- Scaling is all or nothing; one module cannot be scaled independently.
- Boundaries are enforced by discipline and tests rather than by the network, so they can erode if the tests are ignored.

## Consequences

- Architecture tests (`tests/HospitalCore.ArchitectureTests`) are mandatory and must grow with every new module, including a rule that modules do not depend on each other.
- A future ADR must define cross-module communication before the second module is added.
- Database migrations will be organised per module and per schema.
