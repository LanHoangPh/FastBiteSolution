# AI Context README - FastBiteGroup

> Start here when an AI agent or developer needs project context. This file maps each document to its purpose and recommends what to read for each type of task.

---

## 1. Context Map

| Document | Primary Question | Use When |
|---|---|---|
| `PRODUCT_REQUIREMENTS.md` | Why does the product exist? | Understanding product vision, roadmap phase, target users, and business value |
| `PROJECT_CONTEXT.md` | What exists in the solution? | Understanding modules, repository structure, implemented capabilities, and local setup |
| `ARCHITECTURE.md` | How is the system designed? | Making design decisions, checking layer dependencies, CQRS flow, pipeline behavior, and consistency rules |
| `CODING_GUIDELINES.md` | How should code be written? | Implementing features, CRUD, mapping, external libraries, validation, testing, and documentation updates |
| `API_CONTEXT.md` | How does the API behave? | Adding or modifying endpoints, request/response contracts, auth flows, error mapping, and route behavior |
| `DATABASE_CONTEXT.md` | How is data stored? | Changing entities, EF mappings, migrations, MongoDB documents, Redis behavior, or consistency patterns |
| `TECH_STACK.md` | Which tools/packages are used? | Checking framework/package versions, testing tools, observability, Aspire, and infrastructure packages |
| `CURRENT_STATUS.md` | What is done and pending? | Understanding completed work, pending work, test status, known warnings, and migration notes |

---

## 2. Required Baseline For Any Code Change

Before making code changes, read these in order:

1. `PRODUCT_REQUIREMENTS.md`
2. `PROJECT_CONTEXT.md`
3. `ARCHITECTURE.md`
4. `CURRENT_STATUS.md`
5. `CODING_GUIDELINES.md`

This keeps the agent aligned with product direction, current state, architecture constraints, and implementation rules.

---

## 3. Task-Based Reading Guide

### New CRUD Or Business Feature

Read:

1. `PRODUCT_REQUIREMENTS.md`
2. `PROJECT_CONTEXT.md`
3. `ARCHITECTURE.md`
4. `CURRENT_STATUS.md`
5. `CODING_GUIDELINES.md`
6. `API_CONTEXT.md`
7. `DATABASE_CONTEXT.md`

Focus:

- Product phase and business fit.
- Domain entity and use-case boundaries.
- Contract command/query/response shape.
- Application handler and validation rules.
- Persistence model and migration impact.
- Endpoint behavior and error mapping.
- Required tests.

### API Endpoint Change

Read:

1. `PROJECT_CONTEXT.md`
2. `ARCHITECTURE.md`
3. `CODING_GUIDELINES.md`
4. `API_CONTEXT.md`
5. `CURRENT_STATUS.md`

Focus:

- Minimal API endpoint pattern.
- Contract request/response shape.
- `Result<T>` / `ProblemDetails` mapping.
- Authentication and authorization requirements.
- Backward-compatible route behavior.

### Database Or Migration Change

Read:

1. `ARCHITECTURE.md`
2. `CODING_GUIDELINES.md`
3. `DATABASE_CONTEXT.md`
4. `CURRENT_STATUS.md`

Focus:

- PostgreSQL as relational source of truth.
- MongoDB as optional document persistence.
- EF configuration and migration rules.
- No distributed transactions between PostgreSQL and MongoDB.
- Existing migration notes and known drift risks.

### External Library Or Provider Integration

Read:

1. `TECH_STACK.md`
2. `ARCHITECTURE.md`
3. `CODING_GUIDELINES.md`
4. `CURRENT_STATUS.md`

Focus:

- Whether an existing package already solves the need.
- Which layer owns the SDK.
- Options binding and secret handling.
- Abstraction placement.
- Testing and mocking strategy.

### Authentication Or Security Change

Read:

1. `PRODUCT_REQUIREMENTS.md`
2. `ARCHITECTURE.md`
3. `API_CONTEXT.md`
4. `DATABASE_CONTEXT.md`
5. `CODING_GUIDELINES.md`
6. `CURRENT_STATUS.md`

Focus:

- JWT validation and Redis blacklist behavior.
- Refresh token rotation.
- Identity persistence.
- Production-safe error handling.
- Auth endpoint behavior and session revocation rules.

### Documentation-Only Change

Read:

1. `PROJECT_CONTEXT.md`
2. `CODING_GUIDELINES.md`
3. The document being edited

Focus:

- Keep `AGENTS.md` concise.
- Keep project-level technical docs in English.
- Put implementation status in `CURRENT_STATUS.md`.
- Avoid duplicating detailed API/database/package information outside the owning document.

---

## 4. Ownership Rules For Updating Docs

When a change affects project behavior, update the owning document:

| Change Type | Update |
|---|---|
| Product scope, phase, target user, success metric | `PRODUCT_REQUIREMENTS.md` |
| Repository/module overview or local setup | `PROJECT_CONTEXT.md` |
| Dependency direction, architecture decision, pipeline, consistency model | `ARCHITECTURE.md` |
| Coding workflow, review rules, mapping, testing expectations | `CODING_GUIDELINES.md` |
| Routes, request/response, endpoint behavior, auth flow | `API_CONTEXT.md` |
| EF model, migration, MongoDB, Redis, storage guidance | `DATABASE_CONTEXT.md` |
| Package, framework, version, tool | `TECH_STACK.md` |
| Completed work, pending work, known warnings, last verified tests | `CURRENT_STATUS.md` |

Do not copy large sections between documents. Add a short cross-reference instead.

---

## 5. Minimal Context Strategy

Use the smallest context set that is safe:

- For code changes, always include the required baseline.
- For specialized work, add the task-specific documents above.
- Prefer reading the owning document instead of searching across every file.
- Use `CURRENT_STATUS.md` to avoid re-solving completed work or ignoring known migration/test caveats.
- Use `CODING_GUIDELINES.md` as the implementation contract, not as a replacement for architecture, API, database, or status docs.

---

## 6. Default Implementation Checklist

For feature work, confirm:

- Requirement and product fit are clear.
- Layer ownership is correct.
- Commands, queries, and responses live in Contract.
- Handlers and validators live in Application.
- Domain rules stay in Domain.
- EF/Mongo/provider details stay in Persistence or Infrastructure.
- Endpoints stay thin in Presentation.
- Services are registered in the owning layer DI extension.
- Read queries use `AsNoTracking()` and projection.
- Business failures use `Result` errors.
- Tests match the risk.
- Documentation is updated in the owning file.
