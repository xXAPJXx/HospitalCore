# ADR-003: SQL Server and Entity Framework Core

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

Patient data is relational and must honour constraints such as "one patient per identity document". Writes must be transactional. Hospitals commonly run on Microsoft SQL Server or Azure SQL, and the main developer has years of SQL Server and stored procedure experience. Each module must own its data ([ADR-001](ADR-001-modular-monolith.md)), and tests must exercise the real database engine.

## Problem

Which database engine and data access technology should HospitalCore use, how should module data be separated and evolved, and how should persistence be tested?

## Alternatives considered

1. **Database engine**
   - **PostgreSQL**: free, popular in cloud-native stacks. A valid choice, but less common in hospital environments built on the Microsoft stack.
   - **Document database (Cosmos DB, MongoDB)**: flexible schema, but relational integrity and cross-entity constraints become application concerns.
   - **SQL Server**: mature, strong tooling (SSMS), first-class support in .NET and Azure, familiar to the team.
2. **Data access**
   - **Stored procedures with ADO.NET**: full SQL control, but business rules leak into the database and the code is verbose and hard to unit test.
   - **Dapper**: lightweight and fast. It does not track changes or generate migrations, so the team would write every mapping and schema change by hand.
   - **Entity Framework Core**: change tracking, LINQ, migrations and value object mapping. The cost is an abstraction whose generated SQL must be reviewed.
3. **Testing persistence**
   - **EF Core InMemory or SQLite**: fast, but they do not behave like SQL Server (constraints, types, collation), so tests can pass while production fails.
   - **LocalDB**: real engine, but Windows-only, which does not work on Linux CI runners.
   - **Testcontainers with the official SQL Server image**: the real engine in a disposable container on any machine with Docker, including GitHub Actions.

## Decision

- **SQL Server 2025** with **Entity Framework Core 10**.
- **One database, one schema per module.** The Patients module owns the `patients` schema through its own `PatientsDbContext`, and its migration history table lives in that schema.
- **Migrations live in each module's Infrastructure project** and are created and applied with the `dotnet-ef` local tool (pinned in `dotnet-tools.json`). They are applied explicitly (`dotnet ef database update` in development, a deployment step later), never automatically at application start-up.
- **Value objects** are mapped as EF Core complex types. When a value object needs a database index, it is mapped as an owned type instead, because EF Core 10 cannot index complex type properties. `IdentityDocument` is owned for this reason and carries the unique index on document type and number.
- **Enums are stored as strings** so that data remains readable and stable if enum members are reordered.
- **Integration tests use Testcontainers** with the image pinned to `mcr.microsoft.com/mssql/server:2025-CU11-ubuntu-24.04`, matching the local development server.
- **Connection strings**: development uses Windows integrated security on `localhost`, so no secret is stored in the repository. Other environments must provide `ConnectionStrings__HospitalCore` through environment variables or a secret store.

## Advantages

- Database constraints back up business rules: the unique index protects against concurrent duplicate registrations.
- Schema changes are versioned, reviewable code.
- Tests run against the same engine and version as development, both locally and in CI.

## Disadvantages

- Running the test suite requires Docker. The SQL Server image is about 2.2 GB and the first container start adds a few seconds.
- EF Core hides SQL. Generated migrations and queries must be reviewed, for example with `dotnet ef migrations script`.
- Mixing complex and owned types is a framework limitation that contributors must understand.

## Consequences

- Read-heavy scenarios may later use projections or Dapper without changing the write model.
- A unique-constraint violation caused by a race condition currently surfaces as an unhandled `DbUpdateException` (HTTP 500). It should be translated into a conflict result.
- Every new module adds its own schema, `DbContext` and migrations folder.
