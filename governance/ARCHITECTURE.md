# MarcoERP – Architecture Contract

**Deliverable 2: Layer Responsibilities and Explicit Prohibitions**

---

## 1. Architecture Style

MarcoERP follows **Clean Architecture** (Onion Architecture variant) with strict layered boundaries.

```
┌─────────────────────────────────────────────────┐
│                    WpfUI                         │
│            (Presentation Layer)                  │
├─────────────────────────────────────────────────┤
│              Application                         │
│          (Use Case Orchestration)                │
├───────────────────────┬─────────────────────────┤
│    Persistence        │     Infrastructure       │
│   (Data Access)       │   (Cross-Cutting)        │
├───────────────────────┴─────────────────────────┤
│                    Domain                        │
│          (Core Business Logic)                   │
│           *** INNERMOST LAYER ***                │
└─────────────────────────────────────────────────┘
```

---

## 2. Layer Contracts

### 2.1 Domain Layer (`MarcoERP.Domain`)

#### Responsibilities

| #  | Responsibility                                                       |
|----|----------------------------------------------------------------------|
| D1 | Define all business entities (JournalEntry, Account, Product, etc.)  |
| D2 | Define value objects (Money, AccountCode, FiscalPeriod, etc.)        |
| D3 | Define all business enums (AccountType, TransactionStatus, etc.)     |
| D4 | Define repository interfaces (IAccountRepository, etc.)              |
| D5 | Define domain service interfaces (IAuditLogger, etc.)                |
| D6 | Define domain events (JournalPostedEvent, etc.)                     |
| D7 | Define domain exceptions (InsufficientBalanceException, etc.)        |
| D8 | Define domain business rules (DoubleEntryRule, PeriodLockRule, etc.) |
| D9 | Encapsulate entity behavior within entity methods                    |
| D10| Define guard clauses and invariant validation within entities        |

#### What is EXPLICITLY FORBIDDEN in Domain

| #   | Prohibition                                                                   |
|-----|-------------------------------------------------------------------------------|
| DF1 | **NO** reference to Entity Framework Core or any ORM                         |
| DF2 | **NO** reference to any UI framework (WPF, WPF, etc.)                   |
| DF3 | **NO** reference to System.Data.SqlClient or any database driver             |
| DF4 | **NO** reference to any external NuGet package (except pure .NET libraries)   |
| DF5 | **NO** DTOs — domain entities are not data transfer objects                   |
| DF6 | **NO** dependency injection container references                              |
| DF7 | **NO** file I/O, network calls, or any infrastructure concern                |
| DF8 | **NO** static mutable state                                                  |
| DF9 | **NO** reference to Application, Persistence, Infrastructure, or WpfUI       |
| DF10| **NO** `async` methods that depend on infrastructure (e.g., HTTP calls)       |
| DF11| **NO** logging framework references (define interface only)                   |
| DF12| **NO** configuration reading (appsettings, environment variables, etc.)       |

---

### 2.2 Application Layer (`MarcoERP.Application`)

#### Responsibilities

| #  | Responsibility                                                           |
|----|--------------------------------------------------------------------------|
| A1 | Define and execute use cases (PostJournal, CreateInvoice, etc.)          |
| A2 | Define DTOs for input/output across layer boundaries                     |
| A3 | Define validation logic for incoming commands and queries                 |
| A4 | Orchestrate domain entities and repository calls                         |
| A5 | Define application service interfaces (IPostingService, etc.)            |
| A6 | Define application-level exceptions                                      |
| A7 | Map between DTOs and domain entities (via explicit mappers)              |
| A8 | Enforce authorization checks at use-case level                           |
| A9 | Coordinate unit-of-work transactions                                     |
| A10| Trigger domain events when business operations complete                  |
| A11| Define command and query objects for structured request handling          |

#### What is EXPLICITLY FORBIDDEN in Application

| #   | Prohibition                                                                |
|-----|----------------------------------------------------------------------------|
| AF1 | **NO** direct SQL queries or database access                              |
| AF2 | **NO** reference to Entity Framework Core DbContext                       |
| AF3 | **NO** reference to any UI framework                                      |
| AF4 | **NO** concrete repository/infrastructure implementations                 |
| AF5 | **NO** business rule definitions — those belong in Domain                 |
| AF6 | **NO** entity state mutation outside of entity methods                    |
| AF7 | **NO** file I/O or network operations (use interfaces)                   |
| AF8 | **NO** direct logging calls (use injected interface)                     |
| AF9 | **NO** reference to Persistence, Infrastructure, or WpfUI projects       |
| AF10| **NO** UI-specific data formatting (date format, currency display, etc.) |

---

### 2.3 Persistence Layer (`MarcoERP.Persistence`)

#### Responsibilities

| #  | Responsibility                                                         |
|----|------------------------------------------------------------------------|
| P1 | Implement repository interfaces defined in Domain                      |
| P2 | Define EF Core DbContext and entity configurations                     |
| P3 | Define database migrations                                             |
| P4 | Implement Unit of Work pattern                                         |
| P5 | Define EF interceptors (audit trail, soft delete, etc.)                |
| P6 | Implement query optimizations (includes, projections, etc.)            |
| P7 | Manage database connection configuration                               |
| P8 | Implement seed data loading                                            |
| P9 | Manage concurrency token configurations                                |
| P10| Translate domain exceptions to persistence-aware exceptions            |

#### What is EXPLICITLY FORBIDDEN in Persistence

| #   | Prohibition                                                              |
|-----|--------------------------------------------------------------------------|
| PF1 | **NO** business logic of any kind                                       |
| PF2 | **NO** validation beyond database constraint enforcement                |
| PF3 | **NO** reference to any UI framework                                    |
| PF4 | Persistence may reference Application **only** to implement Application-defined service interfaces (e.g., `IAuditLogService`, `ISmartEntryQueryService`). It must NOT contain Application-layer business logic. **Target state**: move cross-cutting interfaces (`ICurrentUserService`, `IDateTimeProvider`) to Domain. |
| PF5 | **NO** reference to Infrastructure layer                                |
| PF6 | **NO** direct instantiation of domain services                          |
| PF7 | **NO** creation of DTOs – persistence works with domain entities only   |
| PF8 | **NO** raw SQL unless explicitly approved and documented                |
| PF9 | **NO** auto-generation of database schema from conventions alone        |
| PF10| **NO** use of lazy loading — all loading must be explicit               |

---

### 2.4 Infrastructure Layer (`MarcoERP.Infrastructure`)

#### Responsibilities

| #  | Responsibility                                                        |
|----|-----------------------------------------------------------------------|
| I1 | Implement audit logging                                               |
| I2 | Implement structured application logging                              |
| I3 | Implement auto-code generation services (invoice numbers, etc.)       |
| I4 | Implement file system operations (export, backup, etc.)               |
| I5 | Implement external report generation                                  |
| I6 | Implement email notification services (if needed)                     |
| I7 | Implement application configuration reading                           |
| I8 | Implement security services (password hashing, etc.)                  |
| I9 | Implement any future API gateway or external integration              |
| I10| Provide time abstraction services (IDateTimeProvider)                 |

#### What is EXPLICITLY FORBIDDEN in Infrastructure

| #   | Prohibition                                                             |
|-----|-------------------------------------------------------------------------|
| IF1 | **NO** business logic of any kind                                      |
| IF2 | **NO** direct database access (no DbContext, no SqlConnection)         |
| IF3 | **NO** reference to Persistence layer                                  |
| IF4 | Infrastructure may reference Application **only** to implement Application-defined service interfaces (e.g., `IBackgroundJobService`). It must NOT contain Application-layer business logic. **Target state**: move cross-cutting interfaces to Domain. |
| IF5 | **NO** reference to WpfUI layer                                        |
| IF6 | **NO** entity state changes — infrastructure does not modify entities  |
| IF7 | **NO** transaction management — that is Persistence's responsibility   |
| IF8 | **NO** DTO creation — infrastructure works with domain types           |
| IF9 | **NO** hardcoded configuration values — must be externally configured  |
| IF10| **NO** UI dialog or message display                                    |

---

### 2.5 WpfUI Layer (`MarcoERP.WpfUI`)

#### Responsibilities

| #   | Responsibility                                                       |
|-----|----------------------------------------------------------------------|
| W1  | Display UI windows and user controls                                 |
| W2  | Capture user input and delegate to Application services              |
| W3  | Display validation errors and messages to the user                   |
| W4  | Implement navigation between windows                                 |
| W5  | Manage window lifecycle and state                                    |
| W6  | Format data for display (dates, currencies, numbers)                 |
| W7  | Host the Dependency Injection composition root (App.xaml.cs)         |
| W8  | Implement view models for complex window data binding                |
| W9  | Handle UI-level exception display (user-friendly messages)           |
| W10 | Manage user session and authentication state                         |
| W11 | Provide UI-level loading indicators and progress feedback            |

#### What is EXPLICITLY FORBIDDEN in WpfUI

| #    | Prohibition                                                            |
|------|------------------------------------------------------------------------|
| WF1  | **NO** business logic — all logic goes through Application layer      |
| WF2  | **NO** direct database access or SQL queries                          |
| WF3  | **NO** direct instantiation of repositories or DbContext              |
| WF4  | **NO** domain entity manipulation — work with DTOs only               |
| WF5  | **NO** calculation of financial amounts (totals, VAT, balances)       |
| WF6  | **NO** code that decides posting rules or fiscal period rules         |
| WF7  | **NO** direct file I/O (use Infrastructure services)                  |
| WF8  | **NO** business validation — only UI input validation (required, format)|
| WF9  | **NO** reference to EF Core or any ORM                                |
| WF10 | **NO** god windows — each window handles one clear responsibility     |
| WF11 | **NO** cross-window data sharing via static variables                 |

---

## 3. Communication Protocol Between Layers

### Data Flow: User Action → Database

```
User clicks "Post Journal"
        │
        ▼
     [WpfUI Window]
   Collects DTO from UI fields
        │
        ▼
   [Application Service]
   Validates DTO
   Loads entities via repository interface
   Executes domain logic via entity methods
   Persists changes via repository interface
   Commits via IUnitOfWork
        │
        ▼
   [Persistence Repository]
   Translates interface call to EF Core operations
   Executes against SQL Server
        │
        ▼
   [Database]
```

### Data Flow: Database → User Display

```
   [Database]
        │
        ▼
   [Persistence Repository]
   Loads entities via EF Core
   Returns domain entities
        │
        ▼
   [Application Service]
   Maps entities to DTOs
   Returns DTOs
        │
        ▼
     [WpfUI Window]
     Formats DTOs for display
     Renders in DataGrid or window fields
```

---

## 4. Cross-Cutting Concern Handling

| Concern              | Responsible Layer      | Mechanism                                  |
|----------------------|------------------------|--------------------------------------------|
| Audit Logging        | Infrastructure         | EF Interceptor + IAuditLogger interface    |
| Application Logging  | Infrastructure         | ILogger interface defined in Domain        |
| Exception Handling   | All (layered)          | Domain → App → UI (each translates)       |
| Transaction Scope    | Persistence            | IUnitOfWork, invoked by Application        |
| Authentication       | WpfUI + Infrastructure | UI captures, Infra validates               |
| Authorization        | Application            | Checked at use-case entry point            |
| Configuration        | Infrastructure         | IConfiguration, read at startup            |
| Code Generation      | Infrastructure         | ICodeGenerator, invoked by Application     |
| Date/Time            | Infrastructure         | IDateTimeProvider (never use DateTime.Now) |
| Caching              | Infrastructure         | ICacheService (if needed)                  |

---

## 5. Contract Violations

Any violation of this architecture contract requires:

1. **Immediate development stop**
2. **Root cause analysis** — why was the violation attempted?
3. **Architectural review** — is the contract wrong, or is the implementation wrong?
4. **Documentation update** — if the contract needs amendment, update this document with justification
5. **Approval** — contract changes must be explicitly approved and documented in VERSIONING.md

---

## 6. Future Extension Points

This architecture is designed to support future extensions without structural changes:

| Future Feature       | Extension Approach                                            |
|----------------------|---------------------------------------------------------------|
| REST API             | New `MarcoERP.API` project, references Application only      |
| Mobile App           | Consumes API layer, no direct Application access             |
| Background Jobs      | New `MarcoERP.Jobs` project, references Application only     |
| Plugin System        | Domain events + Application interfaces for extensibility     |
| Multi-tenancy        | Domain entity extension + Persistence query filters          |
| Report Server        | Infrastructure service implementing IReportGenerator         |

Each new entry point follows the same rule: **reference Application, never bypass it.**

---

## 7. Known Technical Debt

| # | Issue | Target Fix | Status |
|---|-------|------------|--------|
| TD-1 | `ICurrentUserService`, `IDateTimeProvider`, `IAuditLogger` defined in Application but used by Persistence/Infrastructure. Per D5 these belong in Domain. | Move interfaces to `MarcoERP.Domain.Interfaces.Services` | Tracked |
| TD-2 | `AuditLogService` in Persistence creates DTOs (PF7 spirit). Should split: Persistence returns entities, Application maps to DTOs. | Refactor to Application layer | Tracked |
| TD-3 | `ServiceResult<T>` from Application used in some Persistence services. | Define result type in Domain or accept as practical pattern | Tracked |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
| 1.1     | 2026-02-13 | Updated PF4/IF4: acknowledge practical Application dependencies. Added §7 Technical Debt. |
