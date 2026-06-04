# DATABASE_CONTEXT.md - FastBite Solution

## Database Providers

| Provider | Status | Purpose |
|---|---|---|
| PostgreSQL + EF Core/Npgsql | Active primary relational store | Identity, refresh tokens, products, future relational business data |
| MongoDB + MongoDB.Driver | Optional scaffold | Future messages, notifications, delivery logs, projections, outbox documents |
| Redis | Active cache/session store | JWT blacklist and cache entries |

SQL Server is not currently active in the Persistence project.

---

## Relational DbContext

`ApplicationDbContext` lives in `src/backend/FastBiteGroup.Persistence`.

Current role:
- Inherits from ASP.NET Core Identity DbContext with `AppUser`, `AppRole`, and `Guid` keys.
- Applies EF configurations from the Persistence assembly.
- Contains the relational model for products, refresh tokens, and Identity tables.

Relational persistence is registered through:

```csharp
services.AddPostgreSqlPersistence(config);
services.AddIdentityPersistence();
services.AddRepositoryPersistence();
services.AddInterceptorPersistence();
```

Connection string name:

```text
DefaultConnection
```

---

## MongoDB Scaffold

MongoDB code lives under:

```text
src/backend/FastBiteGroup.Persistence/Mongo/
```

Current components:

| Component | Purpose |
|---|---|
| `MongoDbOptions` | `ConnectionString`, `DatabaseName`, `OutboxCollectionName` |
| `MongoDbContext` | Thin wrapper around `IMongoDatabase` |
| `MongoDocumentBase<TKey>` | Base document with `[BsonId]` |
| `MongoOutboxMessageDocument` | Generic Mongo outbox document |
| `MongoIntegrationOutboxStore` | Implements `IIntegrationOutboxStore` |
| `MongoIndexInitializer` | Creates outbox index on `Status` + `OccurredAt` |

MongoDB registration is optional. If no Mongo connection string is configured, `AddMongoPersistence` returns without registering Mongo services.

Accepted configuration keys:

```json
{
  "ConnectionStrings": {
    "mongodb": "mongodb://localhost:27017"
  },
  "MongoDbOptions": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "fastbite",
    "OutboxCollectionName": "integration_outbox"
  }
}
```

`ConnectionStrings:MongoDb` is also accepted.

---

## Current Relational Entities

### `Products`

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Primary key |
| `Name` | `string` | Domain-controlled |
| `Description` | `string` | Domain-controlled |
| `Price` | `decimal` | Must be non-negative |
| Audit fields | various | Created/updated/deleted metadata |

Business rules:
- Created through `Products.Create(...)`.
- Updated through `Update(...)`.
- Invalid price throws domain exception.

### `AppRefreshToken`

| Field | Type | Notes |
|---|---|---|
| `Id` | `long` | Primary key |
| `Token` | `string` | Opaque refresh token |
| `Jti` | `string` | Access token link |
| `UserId` | `Guid` | Identity user id |
| `ExpiresAt` | `DateTime` | Expiry |
| `IsRevoked` | `bool` | Revocation flag |
| `IsUsed` | `bool` | Rotation flag |
| `ReplacedByToken` | `string?` | Rotation chain |

Business rules:
- Active only when not revoked, not used, and not expired.
- Refresh rotation marks old token used.
- Logout/revoke all marks tokens revoked.

### Identity

Identity persistence uses:
- `AppUser`
- `AppRole`
- ASP.NET Core Identity tables with `Guid` keys.

---

## Repository Abstractions

Relational:

```csharp
IRepositoryBase<TEntity, TKey>
IRefreshTokenRepository
IUnitOfWork
```

Mongo/outbox:

```csharp
IIntegrationOutboxStore
IntegrationOutboxMessage
```

`IIntegrationOutboxStore` is intentionally placed in Contract because Persistence implements it and future Application handlers can depend on it without referencing Persistence.

---

## Consistency Guidance

Do not coordinate PostgreSQL and MongoDB with a shared transaction. Instead:

```text
Write source-of-truth data
  -> write outbox message in same DB boundary
  -> commit
  -> background processor reads outbox
  -> update the other store / publish realtime notification
  -> mark processed
```

Future chat example:
- SQL validates `ConversationMember`.
- Mongo inserts `MessageDocument` and `MessageSent` outbox.
- Worker updates SQL conversation summary and pushes notifications.

Use idempotency keys such as `clientMessageId` for retry-safe writes.

---

## Migrations

Existing migrations live in:

```text
src/backend/FastBiteGroup.Persistence/Migrations/
```

Create a migration:

```bash
dotnet ef migrations add <MigrationName> --project src/backend/FastBiteGroup.Persistence --startup-project src/backend/FastBiteGroup.API
```

Apply migrations:

```bash
dotnet ef database update --project src/backend/FastBiteGroup.Persistence --startup-project src/backend/FastBiteGroup.API
```

MongoDB does not use EF migrations. Mongo indexes are created by `MongoIndexInitializer` when Mongo is configured.
