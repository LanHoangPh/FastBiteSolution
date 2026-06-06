# PRODUCT_REQUIREMENTS.md - FastBiteGroup

## 1. Product Vision
**FastBiteGroup** is a scalable, modern .NET 10 Unified Communication Platform designed for enterprises and communities. It acts as a hybrid between Microsoft Teams/Slack (Workspaces, Channels, Video Calls, Role Management, File Attachments) and Facebook Workplace/Yammer (Social Feed, Rich-text Blogs, Comments, Reactions, Polls). 

## 2. Target Audience & Stakeholders
- **Employees / Community Members:** Individuals collaborating via chat, video, and social feeds.
- **Workspace Admins / Moderators:** Managers creating channels, assigning roles, and moderating content (Content Reports).
- **System Operators:** Operations team monitoring system health, managing Redis caching, and overseeing DB scale-out.

## 3. Core Value Proposition
- **Seamless Scalability:** From relational strictness (Workspaces/Users/Roles) to high-volume unstructured data (Chat/Social Feed/Notifications) using a Polyglot Persistence approach (PostgreSQL + MongoDB).
- **Security First:** Robust JWT token management, rotation, and blacklisting ensures safe sessions across multiple devices.
- **Real-time Ready:** Designed from day 1 to handle future high-throughput messaging without crippling the primary transactional database.

## 4. Product Roadmap & Phasing

### Phase 1: Foundation & Identity (Current Phase)
- **Goal:** Establish Clean Architecture, Auth, and Workspaces foundation.
- **Key Features:**
  - JWT Authentication, Registration, Email OTP + Magic Link Activation, Login, Logout, Revoke All.
  - Refresh Token Rotation & Redis Blacklisting.
  - CRUD operations for Workspaces, Channels, and Roles.
- **Success Metrics:** Zero architecture violations, stable PostgreSQL persistence, 100% auth coverage.

### Phase 2: Real-time Chat & Collaboration (Next Step)
- **Goal:** Introduce Chat, Messages, and Notifications leveraging MongoDB & SignalR.
- **Key Features:**
  - Real-time 1:1 and Group Channel messaging (Teams/Slack style).
  - File attachments and basic Video Call integration hooks.
  - Outbox Pattern for eventual consistency between PostgreSQL and MongoDB.
- **Success Metrics:** Low latency message delivery, zero distributed transaction locks.

### Phase 3: Social Feed & Moderation
- **Goal:** Introduce Yammer/Workplace style social interactions.
- **Key Features:** Rich-text Blogs, Comments, Reactions, Polls, and an Admin Dashboard for Content Reports and System Logs.

## 5. Non-Functional Requirements (NFR)
- **Performance:** Sub-200ms response time for standard APIs. Use Redis caching and EF Core `AsNoTracking()`/Projections.
- **Scalability:** Horizontal scaling supported via stateless APIs and Redis token store.
- **Observability:** OpenTelemetry + Serilog for end-to-end distributed tracing.
- **Security:** Strict separation of data, input validation via FluentValidation, no secrets stored in code.
