# PRODUCT_REQUIREMENTS.md - FastBiteGroup

## 1. Product Vision
**FastBiteGroup** is a scalable, modern .NET 10 platform that starts as a Food-Ordering & E-Commerce application and smoothly transitions into a highly interactive, chat-driven ecosystem. The platform aims to bridge the gap between static e-commerce browsing and real-time merchant-customer interactions.

## 2. Target Audience & Stakeholders
- **Customers / End Users:** Individuals looking to browse food products, place orders, and chat in real-time with merchants or support.
- **Merchants / Admins:** Shop owners managing product catalogs, answering customer queries, and processing orders.
- **System Operators:** Operations team monitoring system health, managing Redis caching, and overseeing DB scale-out.

## 3. Core Value Proposition
- **Seamless Scalability:** From relational strictness (Orders/Products) to high-volume unstructured data (Chat/Notifications) using a Polyglot Persistence approach (PostgreSQL + MongoDB).
- **Security First:** Robust JWT token management, rotation, and blacklisting ensures safe sessions across multiple devices.
- **Real-time Ready:** Designed from day 1 to handle future high-throughput messaging without crippling the primary transactional database.

## 4. Product Roadmap & Phasing

### Phase 1: Foundation & E-Commerce (Current Phase)
- **Goal:** Establish Clean Architecture, Auth, and basic Product Management.
- **Key Features:**
  - JWT Authentication, Registration, Login, Logout, Revoke All.
  - Refresh Token Rotation & Redis Blacklisting.
  - CRUD operations for Products with basic domain validations.
- **Success Metrics:** Zero architecture violations, stable PostgreSQL persistence, 100% auth coverage.

### Phase 2: Real-time Communication (Next Step)
- **Goal:** Introduce Chat, Messages, and Notifications leveraging MongoDB.
- **Key Features:**
  - Conversations (1:1 and Group) between Users and Merchants.
  - Real-time Message delivery (SignalR + MongoDB).
  - Outbox Pattern for eventual consistency between PostgreSQL (User/Conversation state) and MongoDB (Message body).
- **Success Metrics:** Low latency message delivery, zero distributed transaction locks.

### Phase 3: Advanced Ordering & Payments
- **Goal:** Full lifecycle order tracking and integration.
- **Key Features:** Cart management, Payment Gateway integration, Delivery tracking.

## 5. Non-Functional Requirements (NFR)
- **Performance:** Sub-200ms response time for standard APIs. Use Redis caching and EF Core `AsNoTracking()`/Projections.
- **Scalability:** Horizontal scaling supported via stateless APIs and Redis token store.
- **Observability:** OpenTelemetry + Serilog for end-to-end distributed tracing.
- **Security:** Strict separation of data, input validation via FluentValidation, no secrets stored in code.
