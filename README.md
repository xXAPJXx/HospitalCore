# HospitalCore

[![CI](https://github.com/xXAPJXx/HospitalCore/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/xXAPJXx/HospitalCore/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**Modern Hospital Information System built with .NET 10.**

HospitalCore is a portfolio project that explores how a hospital information system can be designed with modern .NET engineering practices: a modular monolith, Clean Architecture, Domain-Driven Design and automated quality gates.

> **Status:** early development. The first feature, patient registration and lookup, is implemented end to end with domain, application, API and database tests.

## Table of contents

- [Goals](#goals)
- [Technology stack](#technology-stack)
- [Architecture](#architecture)
- [Modules](#modules)
- [API](#api)
- [Solution structure](#solution-structure)
- [Getting started](#getting-started)
- [Quality gates](#quality-gates)
- [Documentation](#documentation)
- [Disclaimer](#disclaimer)
- [License](#license)

## Goals

- Model realistic hospital workflows (patients, appointments, admissions, laboratory, clinical records, billing) with original business rules.
- Keep module boundaries explicit and enforced by automated tests.
- Apply CQRS and DDD tactical patterns where they provide real value, not by default.
- Treat documentation, testing and architecture decisions as first-class deliverables.

## Technology stack

| Area | In use | Planned |
| --- | --- | --- |
| Language and runtime | C# 14, .NET 10 (LTS) | |
| API | ASP.NET Core Web API (controllers), OpenAPI document, Swagger UI, health checks, ProblemDetails | Authentication and authorization |
| Architecture | Modular monolith, Clean Architecture, DDD tactical patterns, CQRS without a mediator library, Result pattern | Domain events, cross-module contracts |
| Persistence | Entity Framework Core 10, SQL Server 2025, one schema per module | |
| Caching | | Redis |
| Testing | xUnit v3 on Microsoft.Testing.Platform, domain and handler unit tests, `WebApplicationFactory` + Testcontainers (SQL Server 2025) integration tests, NetArchTest architecture tests | |
| Tooling | Central Package Management with lock files, analyzers with warnings as errors, `dotnet format` | |
| Delivery | GitHub Actions continuous integration | Docker, Docker Compose, Azure |

## Architecture

HospitalCore is a **modular monolith**: a single deployable application composed of business modules with explicit boundaries. Inside each module, code follows **Clean Architecture**, so dependencies always point towards the domain.

```mermaid
flowchart TB
    Api["HospitalCore.Api<br/>(host and composition root)"]

    subgraph Patients["Patients module"]
        PI["Infrastructure"] --> PA["Application"] --> PD["Domain"]
    end

    SK["SharedKernel"]

    Api --> PI
    PD --> SK
```

See [docs/architecture/overview.md](docs/architecture/overview.md) for the full picture and [docs/adr](docs/adr/README.md) for the reasoning behind each decision.

## Modules

| Module | Responsibility | Status |
| --- | --- | --- |
| Patients | Patient registration, demographics and identification | Registration and lookup |
| Appointments | Outpatient scheduling | Planned |
| Admissions | Inpatient admissions, transfers, discharges and bed management | Planned |
| Laboratory | Lab orders and results | Planned |
| Clinical Records | Clinical notes and episode history | Planned |
| Billing | Charges and invoicing | Planned |
| Administration | Users, catalogues and configuration | Planned |

Modules are created when their first feature is implemented, not before.

## API

| Method | Route | Description | Responses |
| --- | --- | --- | --- |
| `POST` | `/api/patients` | Register a patient | `201` with `Location` header, `400` validation problem, `409` document already registered |
| `GET` | `/api/patients/{id}` | Get a patient by identifier | `200`, `404` |
| `GET` | `/health` | Liveness check | `200` |

Example registration request:

```json
{
  "givenName": "María",
  "familyName": "García López",
  "dateOfBirth": "1985-03-14",
  "sex": "Female",
  "documentType": "NationalId",
  "documentNumber": "12.345.678-Z"
}
```

Errors follow [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`) and include a stable `code`, for example:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "A patient with the same identity document is already registered.",
  "code": "Patients.DocumentAlreadyRegistered",
  "traceId": "00-…"
}
```

The full contract is available as an OpenAPI document at `/openapi/v1.json` and in Swagger UI at `/swagger` (Development only).

## Solution structure

```text
HospitalCore/
├── docs/
│   ├── adr/                         Architecture Decision Records
│   └── architecture/                Architecture overview and diagrams
├── src/
│   ├── Api/
│   │   └── HospitalCore.Api/        ASP.NET Core host (controllers, middleware, composition root)
│   ├── BuildingBlocks/
│   │   └── HospitalCore.SharedKernel/
│   └── Modules/
│       └── Patients/
│           ├── HospitalCore.Modules.Patients.Domain/
│           ├── HospitalCore.Modules.Patients.Application/
│           └── HospitalCore.Modules.Patients.Infrastructure/
├── tests/
│   ├── HospitalCore.Api.IntegrationTests/       API + SQL Server (Testcontainers)
│   ├── HospitalCore.ArchitectureTests/          Layer dependency rules
│   └── HospitalCore.Modules.Patients.UnitTests/ Domain and handler tests
├── Directory.Build.props            Shared build settings for every project
├── Directory.Packages.props         Central NuGet package versions
├── dotnet-tools.json                Local .NET tools (dotnet-ef)
├── global.json                      Pins the .NET 10 SDK and the test platform
└── HospitalCore.slnx
```

## Getting started

### Prerequisites

- [.NET SDK 10.0.404](https://dotnet.microsoft.com/download/dotnet/10.0) or a later 10.0 feature band. `global.json` pins the SDK, so a machine with .NET 11 installed still builds with .NET 10.
- Git.
- [Docker](https://www.docker.com/products/docker-desktop/), running, to execute the integration tests. They start a SQL Server 2025 container automatically.
- To run the API locally: SQL Server 2025 (any edition, for example Developer) reachable at `localhost` with Windows authentication. To use a different server, set the `ConnectionStrings__HospitalCore` environment variable.
- Optional: Visual Studio 2026, JetBrains Rider or Visual Studio Code with C# Dev Kit.

### Build and test

```bash
git clone https://github.com/xXAPJXx/HospitalCore.git
cd HospitalCore

dotnet build
dotnet test
```

### Create the database and run the API

```bash
dotnet tool restore
dotnet ef database update \
  --project src/Modules/Patients/HospitalCore.Modules.Patients.Infrastructure \
  --startup-project src/Api/HospitalCore.Api \
  --context PatientsDbContext

dotnet run --project src/Api/HospitalCore.Api
```

Run these commands from the repository root so that `global.json` selects the .NET 10 SDK.

With the default `http` launch profile the API listens on `http://localhost:5187`:

| Endpoint | Description |
| --- | --- |
| `GET /health` | Liveness check, returns `Healthy` |
| `GET /openapi/v1.json` | OpenAPI document (Development only) |
| `/swagger` | Swagger UI (Development only) |

To use the `https` profile, trust the ASP.NET Core development certificate once with `dotnet dev-certs https --trust`.

## Quality gates

The [CI workflow](.github/workflows/ci.yml) runs on every push and pull request to `main`. Every change must pass:

- `dotnet restore --locked-mode`, so the build uses exactly the package versions recorded in `packages.lock.json`. NuGet audit runs during restore and, because warnings are errors, fails the build on known vulnerable packages.
- `dotnet format --verify-no-changes`, driven by `.editorconfig`.
- `dotnet build` with **warnings treated as errors** and the `latest-recommended` .NET analyzers.
- `dotnet test`, including architecture tests that fail when a layer or module boundary is broken.

After changing package versions, run `dotnet restore` locally and commit the updated `packages.lock.json` files.

### Branch protection

`main` is protected by a repository ruleset:

- Changes reach `main` only through pull requests, merged with **squash** to keep a linear history.
- The `Build and test` check from GitHub Actions must pass, on a branch that is up to date with `main`.
- Review conversations must be resolved before merging.
- Force pushes and branch deletion are blocked.

Typical workflow:

```bash
git switch -c feature/short-description
# commit your changes
git push
# open a pull request, wait for CI, squash and merge
git switch main && git pull
```

## Documentation

- [Architecture overview](docs/architecture/overview.md)
- [Architecture Decision Records](docs/adr/README.md)
- [Changelog](CHANGELOG.md)

## Disclaimer

HospitalCore is an original work created for learning and portfolio purposes. It is inspired by general hospital management requirements and does not contain proprietary source code, database schemas, documentation or confidential information from any vendor or healthcare organisation. All data used in development and tests is synthetic.

HospitalCore is not a certified medical device and must not be used to manage real patient data.

## License

HospitalCore is released under the [MIT License](LICENSE).
