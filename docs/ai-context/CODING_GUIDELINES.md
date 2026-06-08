# CODING_GUIDELINES.md - FastBite Coding Rules

> Engineering rules for FastBiteGroup. Use this document to review how AI agents and developers should design, implement, test, and document code changes in this repository.

---

## 1. Product And Architecture Mindset

Before writing code, evaluate the request from four perspectives:

- **Business Analyst**: What business problem does this solve? Is it needed in the current product phase?
- **Product Manager**: Does it align with the Unified Communication Platform vision?
- **Product Owner**: Is the API behavior clear for frontend workflows and user journeys?
- **Solution Architect**: Does the design preserve Clean Architecture, CQRS, and the Modular Monolith direction?

Do not implement a feature as "just add a controller, a DbSet, and a service" before identifying the correct layer boundaries, entity or aggregate, command/query contracts, validation rules, storage model, and test scope.

---

## 2. Architecture Rules

FastBiteGroup uses **Clean Architecture** inside a **Modular Monolith**.

Required dependency flow:

```text
Presentation   -> Contract
Application    -> Domain + Contract
Persistence    -> Domain + Contract
Infrastructure -> Application + Domain (+ Persistence only when an adapter needs Identity persistence)
API            -> composition root
```

Rules:

- `Domain` must not depend on any other solution layer.
- `Contract` must not depend on `Application` or `Persistence`.
- `Application` must not depend on `Persistence` or `Infrastructure`.
- `Presentation` must not depend on `Domain` or `Persistence`.
- `API` wires dependencies, middleware, endpoint registration, and configuration.
- EF Core, Redis, MongoDB.Driver, SendGrid, Google SDKs, and other external SDKs must not leak into Application handlers when they can be hidden behind abstractions.
- Endpoints must stay thin and must not contain business logic.

Architecture tests are mandatory. Do not remove, skip, or weaken architecture rules without an explicit request.

---

## 3. Project Layer Responsibilities

| Layer | What belongs here |
|---|---|
| `Domain` | Entities, domain enums, domain exceptions, relational repository abstractions, `IUnitOfWork` |
| `Contract` | Commands, queries, response DTOs, `Result`, shared abstractions used by both Application and Persistence |
| `Application` | MediatR handlers, FluentValidation validators, pipeline behaviors, mapping profiles when needed |
| `Persistence` | EF Core DbContext, EF configurations, repository implementations, MongoDB documents/stores/indexes |
| `Infrastructure` | JWT, Redis cache, email providers, Google auth, external service adapters |
| `Presentation` | Minimal API endpoint definitions |
| `API` | DI composition, middleware pipeline, Swagger, authentication setup, API versioning |

---

## 4. Standard CRUD Workflow

When adding CRUD for a new entity, use the workflow below by default.

### 4.1 Requirement Check

Clarify:

- Which roadmap feature does this entity support?
- Is the entity relational data or document data?
- Is it inside the workspace/tenant boundary?
- Who can create, read, update, delete, archive, or restore it?
- Is deletion hard delete, soft delete, or archive?
- Are there unique constraints?
- Are audit fields required?
- Is caching, outbox messaging, notification, or eventual consistency required?

### 4.2 Domain

Add to `src/backend/FastBiteGroup.Domain`:

- Entity under `Entities`.
- Domain enum when the concept belongs to business rules.
- Domain exception inheriting from `DomainException` when the feature has domain-specific failures.
- Repository abstraction when the generic repository cannot express the use case clearly.

Entity rules:

- Use factory methods such as `Create(...)` when creation has business rules.
- Use behavior methods such as `Update(...)`, `Archive(...)`, or `Join(...)` to protect invariants.
- Avoid broad public setters.
- Do not inject infrastructure dependencies into domain entities.

### 4.3 Contract

Add to `src/backend/FastBiteGroup.Contract/Services/V1/<Feature>`:

- Commands for write flows.
- Queries for read flows.
- Response records for API output.
- Error codes when the feature needs clear conventions.

Contract rules:

- Commands, queries, and responses should be `record` types.
- Commands implement `ICommand<TResponse>` or `ICommand`.
- Queries implement `IQuery<TResponse>`.
- Do not expose EF entities as API responses.

### 4.4 Application

Add to `src/backend/FastBiteGroup.Application`:

- Command handlers under `UseCases/V1/Commands/<Feature>`.
- Query handlers under `UseCases/V1/Queries/<Feature>`.
- Validators under `Validators/<Feature>`.
- AutoMapper profiles under `Mappers` only when mapping reuse justifies them.
- Feature helper classes under `UseCases/V1/<Feature>` only when they are application-level helpers, not domain rules.

Handler rules:

- Always return `Result<T>` or `Result`.
- Do not throw raw exceptions for normal business flow.
- Validate request input through FluentValidation.
- Use repository and unit-of-work abstractions.
- Optimize read queries with projection.
- Do not reference `ApplicationDbContext`, EF repository implementations, Redis concrete services, MongoDB.Driver, or external SDKs directly.

### 4.5 Persistence

Add to `src/backend/FastBiteGroup.Persistence`:

- EF configuration for relational entities.
- Repository implementation when needed.
- `DbSet<>` in `ApplicationDbContext` only with a migration plan.
- EF migration when the relational model changes.
- MongoDB document/store/index initializer when the feature is a document workload.

Relational source-of-truth data:

- Users.
- Authentication and refresh tokens.
- Workspaces, members, permissions, and roles.
- Entities with strong relationships, constraints, or transactional behavior.

MongoDB document workload:

- Chat messages.
- Notifications.
- Delivery logs.
- Read models and projections.
- Outbox document workloads.

Do not create a fake generic repository that tries to cover both EF Core and MongoDB.

### 4.6 Presentation

Add endpoints under `src/backend/FastBiteGroup.Presentation/APIs`.

Endpoints should only:

1. Bind route/body/query/user claims.
2. Create Contract commands or queries.
3. Call `ISender.Send(...)`.
4. Convert `Result` to an HTTP response.

Do not put business rules, EF queries, or direct infrastructure service calls in endpoints.

### 4.7 API Composition

Only change `src/backend/FastBiteGroup.API` when needed for:

- New middleware or configuration.
- New options binding.
- Authentication or authorization setup.
- Endpoint module registration if existing endpoint discovery does not pick it up.

Register new services in the owning layer:

- Application service -> Application DI extension.
- Persistence repository/store -> Persistence DI extension.
- Infrastructure adapter -> Infrastructure DI extension.

---

## 5. Mapping Rules

AutoMapper is allowed, but it is not the default answer for every mapping.

Use AutoMapper when:

- Mapping is repeated across multiple handlers.
- Entity-to-response mapping is straightforward but has many fields.
- A clear profile exists in Application.
- Mapping does not hide business rules.

Do not prefer AutoMapper when:

- Creating or updating a domain entity must go through a factory method or behavior method.
- The mapping has only a few fields and manual mapping is clearer.
- The query needs explicit SQL projection for performance.
- The mapping contains important business logic.

Read queries should usually prefer explicit projection:

```csharp
var result = await query
    .AsNoTracking()
    .Select(x => new ResponseDto(
        x.Id,
        x.Name))
    .ToListAsync(cancellationToken);
```

Use `ProjectTo<T>()` only when:

- The mapping profile is clear.
- The query still translates efficiently to SQL.
- The mapping does not make the query hard to debug or load unnecessary data.

Write flows should prefer domain methods:

```csharp
var entity = Entity.Create(request.Name, request.Description);
entity.Update(request.Name);
```

Do not map commands directly into domain entities when that bypasses domain rules.

---

## 6. External Library Rules

Before adding an external library, provide a trade-off analysis.

Answer:

- What problem does the library solve?
- Is the .NET built-in API sufficient?
- Which layer should own the package?
- Does it affect license, security, performance, or startup time?
- Does it need retry, timeout, logging, or circuit-breaker behavior?
- Where will configuration and secrets come from?
- How will it be tested and mocked?

Defaults:

- External provider SDKs belong in `Infrastructure`.
- Database providers and stores belong in `Persistence`.
- Application depends on interfaces or abstractions only.
- Secrets must not be stored in source-controlled appsettings.
- Do not change existing package versions without an explicit request.

Required pattern:

```text
Application defines or consumes an abstraction
Infrastructure/Persistence implements the abstraction using the external SDK
API binds options from configuration
DI registers the implementation in the owning layer extension
```

Example abstraction:

```csharp
public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken);
}
```

An implementation using Twilio, SendGrid, Firebase, Cloudinary, or another provider belongs in Infrastructure, not in an Application handler.

---

## 7. Data Access And Performance Rules

EF Core read-only queries:

- Must use `AsNoTracking()`.
- Must use projection with `Select(...)` when returning response DTOs.
- Avoid N+1 queries.
- Avoid `Include(...)` when projection can solve the problem.
- Do not load full entities just to return response data.

Write flows:

- Use entity methods or factories to change state.
- Use `IUnitOfWork` for PostgreSQL transaction boundaries.
- Do not use distributed transactions between PostgreSQL and MongoDB.

Cross-store consistency:

```text
Write source-of-truth data
-> write outbox message in the same database boundary
-> commit
-> background processor handles side effects
-> mark processed with idempotency
```

MongoDB is optional:

- The API must start when MongoDB is not configured.
- Add MongoDB features only for real document workloads.

---

## 8. API Rules

The API uses Minimal APIs in the Presentation layer.

Route convention:

```text
/api/v{version:apiVersion}/...
```

Endpoint behavior:

- Auth endpoints belong to the auth module.
- Workspace endpoints must respect the workspace/tenant boundary.
- Protected endpoints require JWT when they expose user, workspace, or product data.
- Errors should be returned as `ProblemDetails`.
- Do not expose production error details.

Result mapping:

- Handlers return `Result<T>` or `Result`.
- Endpoints call `HandleFailure(result)` on failure.
- Current status convention:
  - `NotFound` -> 404
  - `Conflict` -> 409
  - `Unauthorized` -> 401
  - `Forbidden` -> 403
  - default -> 400

---

## 9. Validation And Error Rules

Validation:

- Use FluentValidation for command/query input.
- Validators belong in Application.
- Validation failures flow through the validation pipeline behavior.

Business errors:

- Use `Result.Failure(...)`.
- Error codes should use a feature prefix, for example `Workspace.NotFound`.
- Domain exceptions are for invariant or domain-level exceptional cases.
- Application handlers should not throw raw exceptions for normal business flow.

Security errors:

- Do not leak sensitive information.
- Logout must read JWT `jti` from claims instead of trusting the request body.
- Refresh token rotation must mark the old token as used and issue a new token.

---

## 10. Testing Rules

When adding a feature, choose test coverage based on risk:

- New domain rule -> Domain tests.
- New handler behavior -> Application tests.
- New validator -> Contract/Application validator tests following existing patterns.
- Important API or database flow -> Integration tests when live PostgreSQL/Redis is available.
- Dependency direction change -> Architecture tests are mandatory.

Minimum verification:

```bash
dotnet build FastBiteSolution.slnx
dotnet test tests/backend/FastBiteGroup.Architecture.Tests/FastBiteGroup.Architecture.Tests.csproj
```

When changing a specific feature, also run relevant test projects:

```bash
dotnet test tests/backend/FastBiteGroup.Application.Tests/FastBiteGroup.Application.Tests.csproj
dotnet test tests/backend/FastBiteGroup.Contract.Tests/FastBiteGroup.Contract.Tests.csproj
dotnet test tests/backend/FastBiteGroup.Domain.Tests/FastBiteGroup.Domain.Tests.csproj
```

Integration tests may require live PostgreSQL and Redis.

---

## 11. Migration Rules

When changing the relational model:

- Have a migration plan.
- Do not change `ApplicationDbContext` or add `DbSet<>` without identifying the migration impact.
- Prefer EF configurations under Persistence `Configurations`.
- Place migrations under `src/backend/FastBiteGroup.Persistence/Migrations`.
- Do not suppress `PendingModelChangesWarning`.

Commands:

```bash
dotnet ef migrations add <MigrationName> --project src/backend/FastBiteGroup.Persistence --startup-project src/backend/FastBiteGroup.API
dotnet ef database update --project src/backend/FastBiteGroup.Persistence --startup-project src/backend/FastBiteGroup.API
```

MongoDB does not use EF migrations. Index and document setup belongs in Mongo persistence code.

---

## 12. Documentation Rules

When project behavior or status changes:

- Keep `AGENTS.md` concise.
- Put detailed status updates in `docs/ai-context/CURRENT_STATUS.md`.
- Update `API_CONTEXT.md` when adding significant API behavior.
- Update `DATABASE_CONTEXT.md` when adding important database/provider/migration behavior.
- Update `TECH_STACK.md` when adding important packages or frameworks.
- Update `ARCHITECTURE.md` when changing an architecture decision.

Documentation should be:

- Clear enough to guide future implementation without reading unrelated code first.
- Specific about paths, commands, layer ownership, and constraints.
- Short enough that an agent or developer can scan it before making a change.
- Written in English for project-level technical docs unless a file is intentionally user-facing Vietnamese documentation.

---

## 13. Code Style Rules

Defaults:

- Commands, queries, and responses use `record`.
- Domain entities use classes with private setters when invariants need protection.
- Async methods accept `CancellationToken`.
- Type and file names should describe the feature and use case.
- Comments should be short and used only where the code is not self-explanatory.
- Do not hardcode secrets, connection strings, or license keys.
- Do not refactor outside the task scope unless required for a safe implementation.
- Do not change NuGet package versions without an explicit request.

Format and maintainability:

- Prefer clear code over premature abstraction.
- Add abstractions only when they reduce real duplication or protect dependency boundaries.
- Use the options pattern for configuration.
- Register services in the correct layer's DI extension.
- Do not put DTOs in Domain.
- Do not expose EF entities as API responses.

---

## 14. Definition Of Done

A code change is done when:

- It solves the requested requirement without unnecessary scope expansion.
- It preserves Clean Architecture dependency rules.
- It includes appropriate validation and error handling.
- It has clear mapping and does not load unnecessary data in read queries.
- It registers services in the correct layer.
- It has a migration plan when the relational model changes.
- It includes tests proportional to the risk.
- `dotnet build` passes, or the reason it could not be run is documented.
- Architecture tests pass, or the reason they could not be run is documented.
- Documentation is updated when behavior or project status changes materially.

---

## 15. Default Response Format For Design/Code Work

When explaining or proposing code, use this structure:

1. **Problem Analysis**
2. **Proposed Architecture**
3. **Implementation**
4. **Code**
5. **Best Practices**

Always call out:

- Trade-offs.
- Pros and cons.
- Why this approach was chosen.
- Which tests should run.
- Which files and layers are affected.
