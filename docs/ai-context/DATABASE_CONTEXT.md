# DATABASE_CONTEXT.md — FastBite Solution

## Database Provider

| Provider | Status | Notes |
|---|---|---|
| **PostgreSQL** (Npgsql) | Primary (Aspire-provisioned) | Used in dev via Aspire |
| **SQL Server** | Configured in DI, not active | `AddSqlServerPersistence()` code exists |

> ⚠️ **Inconsistency**: `ServiceCollectionExtensions.AddSqlServerPersistence()` configures SQL Server retry strategy, but PostgreSQL NuGet is also present. The active provider should be consolidated.

---

## DbContext

### `ApplicationDbContext` (`FastBiteGroup.Persistence`)

```csharp
public sealed class ApplicationDbContext : DbContext
{
    // ⚠️ EMPTY — No DbSet<> properties defined yet
    // No entity configurations applied
    // No migrations exist
}
```

**Status**: Scaffold only. Must be populated before any persistence operations can work.

**Required next steps**:
1. Add `DbSet<Products>` and `DbSet<RefreshToken>`.
2. Register `AppUser` + ASP.NET Core Identity tables (commented out in DI).
3. Apply entity configurations from `Configurations/` folder.
4. Run `dotnet ef migrations add Initial`.

---

## Entities

### `Products` (FastBiteGroup.Domain.Entities)

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | PK (from `EntityAuditBase<int>`) |
| `Name` | `string` | Private setter — domain-controlled |
| `Description` | `string` | Private setter |
| `Price` | `decimal` | Private setter — validated ≥ 0 |
| `CreatedAt` | `DateTimeOffset` | Audit field |
| `UpdatedAt` | `DateTimeOffset?` | Audit field |
| `CreatedBy` | `Guid` | Audit field (user ID) |
| `UpdatedBy` | `Guid?` | Audit field |
| `IsDeleted` | `bool` | Soft delete flag |
| `DeletedAt` | `DateTimeOffset?` | Soft delete timestamp |

**Business rules**:
- `Price` must be ≥ 0 (enforced by `ProductPriceInvalidException`).
- Instantiated only via `Products.Create(name, description, price)` factory method.
- Updated via `Update(name, description, price)`.

**Table name constant**: `TableNames.Product = "Product"`.

---

### `RefreshToken` (FastBiteGroup.Domain.Entities)

| Field | Type | Notes |
|---|---|---|
| `Id` | `long` | PK (from `EntityBase<long>`) |
| `Token` | `string` | Opaque random token value |
| `Jti` | `string` | Links to corresponding Access Token |
| `UserId` | `Guid` | ASP.NET Core Identity User ID |
| `ExpiresAt` | `DateTime` | Token expiry |
| `IsRevoked` | `bool` | Explicitly revoked |
| `IsUsed` | `bool` | Consumed in rotation |
| `ReplacedByToken` | `string?` | Token that replaced this one (rotation chain) |

**Business rules**:
- `IsActive = !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow`
- `MarkUsed(replacementToken)` — sets `IsUsed = true`, records replacement.
- `Revoke()` — sets `IsRevoked = true`.

**Table name constant**: `TableNames.RefreshTokens = "RefreshTokens"`.

---

### Identity Entities (Planned — Commented Out)

From `TableNames.cs`, the following tables are planned for ASP.NET Core Identity:

| Table Constant | Purpose |
|---|---|
| `AppUser` | Identity user |
| `AppRoles` | Identity roles |
| `AppUserRoles` | User-role join |
| `AppUserClaims` | IdentityUserClaim |
| `AppRoleClaims` | IdentityRoleClaim |
| `AppUserLogins` | External logins |
| `AppUserTokens` | Identity tokens |

Identity configuration is **fully commented out** in `Persistence/DependencyInjection/Extensions/ServiceCollectionExtensions.cs`.

---

## Entity Hierarchy

```
EntityBase<TKey>
    └── EntityAuditBase<TKey>    ← CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, DeletedAt
            └── Products<int>    ← Business entity with soft delete + audit

EntityBase<TKey>
    └── RefreshToken<long>       ← Token entity (no audit fields)
```

---

## Repository Abstractions

### `IRepositoryBase<TEntity, TKey>` (Domain)

```csharp
Task<TEntity> FindByIdAsync(TKey id, CancellationToken ct, params Expression<Func<TEntity, object>>[] includes);
Task<TEntity> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct, params Expression<Func<TEntity, object>>[] includes);
IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null, params Expression<Func<TEntity, object>>[] includes);
void Add(TEntity entity);
void Update(TEntity entity);
void Remove(TEntity entity);
void RemoveMultiple(List<TEntity> entities);
```

### `IRefreshTokenRepository` (Domain)

Extends `IRepositoryBase<RefreshToken, long>` with:
```csharp
Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken ct = default);
Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
```

---

## Repository Implementation Status

| Class | Status | Notes |
|---|---|---|
| `RepositoryBase<TEntity, TKey>` | ⚠️ STUB | All methods throw `NotImplementedException` |
| `EFUnitOfWork` | ⚠️ STUB | `SaveChangesAsync`, `ExecuteTransactionAsync`, `DisposeAsync` all throw |
| `IRefreshTokenRepository` implementation | ❌ Missing | Interface exists, no concrete class |

---

## DI Registration

```csharp
// In Persistence DI:
services.AddTransient(typeof(IUnitOfWork), typeof(EFUnitOfWork));
services.AddTransient(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));

// DbContext Pool (SQL Server path):
services.AddDbContextPool<DbContext, ApplicationDbContext>((provider, builder) => {
    builder
        .UseLazyLoadingProxies(true)
        .UseSqlServer(connectionString, opts => opts.ExecutionStrategy(
            deps => new SqlServerRetryingExecutionStrategy(deps, maxRetry, maxDelay, errorNumbers)))
        .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
});
```

---

## ERD (Current State)

```mermaid
erDiagram
    Products {
        int Id PK
        string Name
        string Description
        decimal Price
        DateTimeOffset CreatedAt
        DateTimeOffset UpdatedAt_nullable
        Guid CreatedBy
        Guid UpdatedBy_nullable
        bool IsDeleted
        DateTimeOffset DeletedAt_nullable
    }

    RefreshToken {
        long Id PK
        string Token
        string Jti
        Guid UserId FK
        DateTime ExpiresAt
        bool IsRevoked
        bool IsUsed
        string ReplacedByToken_nullable
    }

    AppUser {
        Guid Id PK
        string Email
        string FirstName
        string LastName
        string PasswordHash
        note "Planned - ASP.NET Identity"
    }

    AppUser ||--o{ RefreshToken : "has"
```

---

## Migrations

**No migrations exist yet.**

To create the first migration:
```bash
dotnet ef migrations add Initial \
  --project FastBiteGroup.Persistence \
  --startup-project FastBiteGroup.API \
  --output-dir Migrations
```

**Prerequisites before running migrations**:
1. Register all entity `DbSet<>` in `ApplicationDbContext`.
2. Add `IEntityTypeConfiguration<T>` for each entity in `Configurations/` folder.
3. Decide on Identity: add `IdentityDbContext` or keep custom.
4. Resolve PostgreSQL vs SQL Server provider choice.

---

## Connection String Configuration

In development (via .NET Aspire):
- Aspire injects `ConnectionStrings__DefaultConnection` automatically.

In production:
- Must be set via environment variable: `ConnectionStrings__DefaultConnection`.
- Never stored in source-controlled `appsettings.json`.
