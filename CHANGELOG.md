# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Patients module: register a patient (`POST /api/patients`) and get a patient by identifier (`GET /api/patients/{id}`).
  - `Patient` aggregate with `PersonName` and `IdentityDocument` value objects and a strongly typed `PatientId`.
  - Business rules: required and normalised names, date of birth not in the future and at most 150 years ago, normalised identity document unique per document type.
  - CQRS command and query handlers returning `Result` values with stable error codes, mapped to RFC 9457 ProblemDetails.
  - EF Core 10 persistence in the `patients` schema of SQL Server, with the `InitialPatients` migration.
- Shared kernel primitives: `Result`, `Error`, `Entity` and command and query handler interfaces.
- Unit tests for the Patients domain and application handlers, and API integration tests against SQL Server 2025 with Testcontainers.
- `dotnet-ef` local tool manifest.
- ADR-003 (SQL Server and EF Core), ADR-004 (CQRS) and ADR-005 (Domain-Driven Design).

- Solution scaffolding for a modular monolith on .NET 10: API host, shared kernel and the Patients module (Domain, Application and Infrastructure projects).
- `global.json` pinning the .NET 10 SDK and enabling Microsoft.Testing.Platform for `dotnet test`.
- Shared build settings (`Directory.Build.props`) with nullable reference types, warnings as errors and recommended analyzers.
- Central Package Management (`Directory.Packages.props`).
- API baseline: controllers, OpenAPI document, Swagger UI, health check endpoint and ProblemDetails error responses.
- Architecture tests enforcing the Clean Architecture dependency rule with NetArchTest.
- API integration tests with `WebApplicationFactory`.
- README, architecture overview, ADR-001 (modular monolith) and ADR-002 (Clean Architecture).
- GitHub Actions CI workflow: locked restore, format check, build and tests on every push and pull request to `main`.
- NuGet lock files (`packages.lock.json`) for reproducible restores and CI caching.

### Fixed

- Concurrent registrations of the same identity document now return `409 Conflict` instead of `500`. SQL Server unique-key errors (2601, 2627) are translated into a `UniqueConstraintViolationException` by the Patients unit of work and mapped to `Patients.DocumentAlreadyRegistered` by the handler.
- Branch protection documentation for the `main` ruleset (pull requests, required CI check, linear history).
- MIT license.
