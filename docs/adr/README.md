# Architecture Decision Records

An Architecture Decision Record (ADR) captures one significant decision, the context in which it was made and its consequences. ADRs are immutable once accepted: when a decision changes, a new ADR supersedes the old one.

## Index

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-001](ADR-001-modular-monolith.md) | Modular monolith architecture | Accepted |
| [ADR-002](ADR-002-clean-architecture.md) | Clean Architecture inside each module | Accepted |
| ADR-003 | SQL Server and Entity Framework Core | Planned |
| ADR-004 | CQRS | Planned |
| ADR-005 | Domain-Driven Design | Planned |
| ADR-006 | Authentication and authorization | Planned |

## Writing a new ADR

1. Copy [template.md](template.md) to `ADR-NNN-short-title.md`.
2. Fill in every section, including the alternatives that were rejected and why.
3. Add the ADR to the index above.
