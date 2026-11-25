# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Solution scaffolding for a modular monolith on .NET 10: API host, shared kernel and the Patients module (Domain, Application and Infrastructure projects).
- `global.json` pinning the .NET 10 SDK and enabling Microsoft.Testing.Platform for `dotnet test`.
- Shared build settings (`Directory.Build.props`) with nullable reference types, warnings as errors and recommended analyzers.
- Central Package Management (`Directory.Packages.props`).
- API baseline: controllers, OpenAPI document, Swagger UI, health check endpoint and ProblemDetails error responses.
- Architecture tests enforcing the Clean Architecture dependency rule with NetArchTest.
- API integration tests with `WebApplicationFactory`.
- README, architecture overview, ADR-001 (modular monolith) and ADR-002 (Clean Architecture).
