# MarcoERP – Solution Structure

**Deliverable 1: Final Solution Layout, Dependency Direction Rules, Forbidden Cross-Layer Access**

---

## 1. Solution Layout

```
MarcoERP/
│
├── MarcoERP.sln
│
├── src/
│   ├── MarcoERP.Domain/
│   │   ├── Entities/
│   │   │   ├── Accounting/
│   │   │   ├── Inventory/
│   │   │   ├── Sales/
│   │   │   ├── Purchasing/
│   │   │   ├── Security/
│   │   │   ├── Treasury/
│   │   │   ├── Settings/
│   │   │   └── Common/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   │   ├── Repositories/       (IAccountRepository, etc.)
│   │   │   ├── Security/           (IUserRepository, IRoleRepository)
│   │   │   ├── Settings/           (ISystemSettingRepository, etc.)
│   │   │   ├── Inventory/          (IProductRepository, etc.)
│   │   │   ├── Sales/              (ISalesInvoiceRepository, etc.)
│   │   │   ├── Purchases/          (IPurchaseInvoiceRepository, etc.)
│   │   │   └── Treasury/           (ICashboxRepository, etc.)
│   │   └── Exceptions/
│   │
│   ├── MarcoERP.Application/
│   │   ├── Interfaces/
│   │   │   ├── Accounting/
│   │   │   ├── Inventory/
│   │   │   ├── Sales/
│   │   │   ├── Purchases/
│   │   │   ├── Treasury/
│   │   │   ├── Security/
│   │   │   ├── Settings/
│   │   │   ├── Reports/
│   │   │   ├── SmartEntry/
│   │   │   └── Search/
│   │   ├── Services/
│   │   │   ├── Accounting/
│   │   │   ├── Inventory/
│   │   │   ├── Sales/
│   │   │   ├── Purchasing/
│   │   │   ├── Treasury/
│   │   │   ├── Security/
│   │   │   ├── Reports/
│   │   │   └── Common/
│   │   ├── DTOs/
│   │   │   ├── Accounting/
│   │   │   ├── Inventory/
│   │   │   ├── Sales/
│   │   │   ├── Purchasing/
│   │   │   ├── Treasury/
│   │   │   ├── Security/
│   │   │   ├── Settings/
│   │   │   └── Common/
│   │   ├── Common/                    (ServiceResult, AuthorizationGuard, PermissionKeys)
│   │   ├── Validators/
│   │   ├── Mappers/
│   │   └── Reporting/
│   │
│   ├── MarcoERP.Persistence/
│   │   ├── Context/
│   │   ├── Configurations/
│   │   │   ├── Accounting/
│   │   │   ├── Inventory/
│   │   │   ├── Sales/
│   │   │   ├── Purchasing/
│   │   │   └── Common/
│   │   ├── Repositories/
│   │   ├── Migrations/
│   │   ├── Seeds/
│   │   └── Interceptors/
│   │
│   ├── MarcoERP.Infrastructure/
│   │   ├── Logging/
│   │   ├── AuditLog/
│   │   ├── FileSystem/
│   │   ├── Reporting/
│   │   ├── CodeGeneration/
│   │   ├── Security/
│   │   └── Configuration/
│   │
│   └── MarcoERP.WpfUI/
│       ├── App.xaml / App.xaml.cs         (DI composition root)
│       ├── appsettings.json
│       ├── Common/                        (F1SearchBehavior, UiBehaviors, helpers)
│       ├── Converters/                    (Value converters)
│       ├── Navigation/                    (IDirtyStateAware, INavigationAware, DirtyStateGuard, NavigationService, IViewRegistry)
│       ├── Services/                      (WindowService, InvoicePdfPreviewService, QuickTreasuryDialogService)
│       ├── Themes/                        (Material Design themes)
│       ├── Resources/                     (Icons, images)
│       ├── ViewModels/
│       │   ├── BaseViewModel.cs / RelayCommand.cs
│       │   ├── IInvoiceLineFormHost.cs    (Shared host interface for popup)
│       │   ├── Common/                    (InvoiceLinePopupState — shared popup state)
│       │   ├── Sales/                     (SalesInvoice/Return/Quotation VMs)
│       │   ├── Purchases/                 (PurchaseInvoice/Return/Quotation VMs)
│       │   ├── Inventory/                 (Product, Category, Warehouse VMs)
│       │   ├── Treasury/                  (CashReceipt/Payment/Transfer/Cashbox VMs)
│       │   ├── Accounting/                (JournalEntry, ChartOfAccounts VMs)
│       │   ├── Reports/                   (StockCard, Sales/Purchase/Inventory Reports)
│       │   ├── Settings/                  (UserManagement, SystemSettings VMs)
│       │   └── Shell/                     (MainWindowVM, LoginVM)
│       └── Views/
│           ├── Common/                    (InvoiceAddLineWindow, SearchLookupWindow, QuickTreasuryDialog, InvoicePdfPreviewDialog)
│           ├── Sales/                     (Invoice/Return/Quotation list + detail views)
│           ├── Purchases/                 (Invoice/Return/Quotation list + detail views)
│           ├── Inventory/                 (Product, Category, Warehouse, BulkPriceUpdate views)
│           ├── Treasury/                  (CashReceipt/Payment/Transfer/Cashbox views)
│           ├── Accounting/                (JournalEntry, ChartOfAccounts views)
│           ├── Reports/                   (All report views)
│           ├── Settings/                  (UserManagement, SystemSettings views)
│           └── Shell/                     (MainWindow, LoginWindow)
│
├── tests/
│   ├── MarcoERP.Domain.Tests/
│   ├── MarcoERP.Application.Tests/
│   ├── MarcoERP.Persistence.Tests/
│   └── MarcoERP.Integration.Tests/
│
├── governance/
│   ├── SOLUTION_STRUCTURE.md          (this file)
│   ├── ARCHITECTURE.md
│   ├── PROJECT_RULES.md
│   ├── DATABASE_POLICY.md
│   ├── UI_GUIDELINES.md
│   ├── AGENT_POLICY.md
│   ├── VERSIONING.md
│   ├── SECURITY_POLICY.md
│   ├── ACCOUNTING_PRINCIPLES.md
│   ├── FINANCIAL_ENGINE_RULES.md
│   ├── RECORD_PROTECTION_POLICY.md
│   ├── RISK_PREVENTION_FRAMEWORK.md
│   └── AGENT_CONTROL_SYSTEM.md
│
├── README.md
└── .gitignore
```

---

## 2. Project Descriptions

| Project                    | Type          | Purpose                                                        |
|----------------------------|---------------|----------------------------------------------------------------|
| MarcoERP.Domain            | Class Library | Entities, value objects, enums, domain interfaces, domain rules |
| MarcoERP.Application       | Class Library | Use cases, DTOs, validators, service orchestration             |
| MarcoERP.Persistence       | Class Library | EF Core DbContext, repositories, migrations, configurations    |
| MarcoERP.Infrastructure    | Class Library | Cross-cutting: logging, audit, file I/O, code generation       |
| MarcoERP.WpfUI             | WPF App      | All UI windows, views, view models, user interaction       |
| MarcoERP.Domain.Tests      | Test (xUnit)  | Unit tests for domain logic                                    |
| MarcoERP.Application.Tests | Test (xUnit)  | Unit tests for application services and validators             |
| MarcoERP.Persistence.Tests | Test (xUnit)  | Integration tests for repository and EF configurations         |
| MarcoERP.Integration.Tests | Test (xUnit)  | End-to-end workflow tests                                      |

---

## 3. Dependency Direction Rules

### Allowed Dependencies (Arrows point from consumer → dependency)

```
MarcoERP.WpfUI ──────────► MarcoERP.Application
MarcoERP.WpfUI ──────────► MarcoERP.Infrastructure  (DI registration only)
MarcoERP.WpfUI ──────────► MarcoERP.Persistence     (DI registration only)
MarcoERP.Application ────► MarcoERP.Domain
MarcoERP.Persistence ────► MarcoERP.Domain
MarcoERP.Persistence ────► MarcoERP.Application     (interface implementation only — see TD-1)
MarcoERP.Infrastructure ─► MarcoERP.Domain
MarcoERP.Infrastructure ─► MarcoERP.Application     (interface implementation only — see TD-1)
```

> **Technical Debt (TD-1):** Persistence and Infrastructure reference Application to implement Application-defined interfaces. Target state: move cross-cutting interfaces (`ICurrentUserService`, `IDateTimeProvider`, `IAuditLogger`) to Domain per ARCHITECTURE.md D5. See ARCHITECTURE.md §7 for full tracking.

### Dependency Flow Diagram

```
┌──────────────────────────────────────────────────────┐
│                    MarcoERP.WpfUI                     │
│              (Presentation Layer)                     │
└──────┬───────────────┬──────────────┬────────────────┘
       │               │              │
       │ (uses)        │ (DI only)    │ (DI only)
       ▼               ▼              ▼
┌──────────────┐ ┌───────────────┐ ┌─────────────────┐
│  Application │ │ Infrastructure│ │   Persistence   │
│   (Use Cases)│ │(Cross-Cutting)│ │  (Data Access)  │
└──────┬───────┘ └──────┬────────┘ └──────┬──────────┘
       │                │                 │
       │ (uses)         │ (implements)    │ (implements)
       ▼                ▼                 ▼
┌──────────────────────────────────────────────────────┐
│                   MarcoERP.Domain                     │
│        (Entities, Interfaces, Rules, Enums)           │
│              *** DEPENDS ON NOTHING ***                │
└──────────────────────────────────────────────────────┘
```

### The Golden Rule

> **All dependencies point inward toward the Domain.**
> The Domain layer has ZERO dependencies on any other project.

---

## 4. Forbidden Cross-Layer Access

### Absolute Prohibitions

| FROM                  | TO                     | Status       | Reason                                      |
|-----------------------|------------------------|--------------|---------------------------------------------|
| Domain                | Application            | **FORBIDDEN** | Domain must remain pure and self-contained  |
| Domain                | Persistence            | **FORBIDDEN** | Domain must not know about data storage     |
| Domain                | Infrastructure         | **FORBIDDEN** | Domain must not depend on external services  |
| Domain                | WpfUI                  | **FORBIDDEN** | Domain must not know about presentation     |
| Application           | Persistence            | **FORBIDDEN** | Application uses interfaces, not implementations |
| Application           | Infrastructure         | **FORBIDDEN** | Application uses interfaces, not implementations |
| Application           | WpfUI                  | **FORBIDDEN** | Application must not know about UI          |
| Persistence           | Application            | **FORBIDDEN** | Data layer must not call business logic     |
| Persistence           | Infrastructure         | **FORBIDDEN** | Data layer must not depend on infrastructure |
| Persistence           | WpfUI                  | **FORBIDDEN** | Data layer must not know about UI           |
| Infrastructure        | Application            | **FORBIDDEN** | Infrastructure must not call business logic |
| Infrastructure        | Persistence            | **FORBIDDEN** | Infrastructure must not access data directly |
| Infrastructure        | WpfUI                  | **FORBIDDEN** | Infrastructure must not know about UI       |

### WpfUI Special Rules

| WpfUI Access To       | Status                  | Condition                                    |
|-----------------------|-------------------------|----------------------------------------------|
| Application           | **ALLOWED**             | Via service interfaces only                  |
| Persistence           | **DI REGISTRATION ONLY**| Only in composition root (App.xaml.cs)       |
| Infrastructure        | **DI REGISTRATION ONLY**| Only in composition root (App.xaml.cs)       |
| Domain                | **FORBIDDEN**           | Must go through Application layer            |

---

## 5. Interface Ownership Rule

> **Interfaces are defined where they are NEEDED, not where they are IMPLEMENTED.**

| Interface Defined In | Implemented In    | Example                             |
|----------------------|-------------------|-------------------------------------|
| Domain               | Persistence       | `IAccountRepository`                |
| Domain               | Infrastructure    | `IAuditLogger`                      |
| Application          | Infrastructure    | `IReportGenerator`                  |
| Application          | Persistence       | `IUnitOfWork`                       |

This ensures the Dependency Inversion Principle is enforced. The Domain and Application layers define contracts; Persistence and Infrastructure fulfill them.

---

## 6. Composition Root

The **only** place where all layers are wired together is `MarcoERP.WpfUI/App.xaml.cs`.

This is the single entry point for:
- Dependency Injection container setup
- Registering repository implementations from Persistence
- Registering service implementations from Infrastructure
- Configuring the EF Core DbContext
- Launching the main application window

**No other location** may reference concrete implementations across layer boundaries.

---

## 7. Namespace Convention

```
MarcoERP.{Layer}.{Module}.{SubModule}
```

Examples:
```
MarcoERP.Domain.Entities.Accounting.JournalEntry
MarcoERP.Application.Services.Accounting.PostingService
MarcoERP.Persistence.Configurations.Accounting.JournalEntryConfig
MarcoERP.WpfUI.Views.Accounting.JournalEntryWindow
MarcoERP.Domain.Interfaces.Repositories.IJournalEntryRepository
```

---

## 8. File Naming Convention

| Item            | Convention                    | Example                          |
|-----------------|-------------------------------|----------------------------------|
| Entity          | PascalCase, singular noun     | `JournalEntry.cs`                |
| Interface       | `I` + PascalCase              | `IJournalEntryRepository.cs`     |
| Service         | PascalCase + `Service`        | `PostingService.cs`              |
| DTO             | PascalCase + `Dto`            | `JournalEntryDto.cs`             |
| Validator       | PascalCase + `Validator`      | `JournalEntryValidator.cs`       |
| Configuration   | PascalCase + `Configuration`  | `JournalEntryConfiguration.cs`   |
| Window          | PascalCase + `Window`         | `JournalEntryWindow.xaml`        |
| ViewModel       | PascalCase + `ViewModel`      | `JournalEntryViewModel.cs`       |
| Test            | PascalCase + `Tests`          | `PostingServiceTests.cs`         |

---

## 9. Test Project Rules

- **Domain.Tests**: Pure unit tests. No database, no mocking infrastructure. Test business rules only.
- **Application.Tests**: Unit tests with mocked repositories. Test orchestration and validation logic.
- **Persistence.Tests**: Use in-memory or local SQL Server for testing EF configurations and queries.
- **Integration.Tests**: Test full workflows across layers. Use real database instance.

---

## 10. Build Order Enforcement

The solution MUST build in this order without circular references:

```
1. MarcoERP.Domain          (zero dependencies)
2. MarcoERP.Application     (depends on Domain)
3. MarcoERP.Persistence     (depends on Domain, Application)
4. MarcoERP.Infrastructure  (depends on Domain, Application)
5. MarcoERP.WpfUI           (depends on Application, references Persistence & Infrastructure for DI)
```

If a build order violation is detected, development MUST STOP until resolved.

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
| 1.1     | 2026-02-11 | Added detailed WpfUI structure        |
| 1.2     | 2026-02-13 | Updated §1 Domain/Application structure to match reality. Removed non-existent ValueObjects/Events/Rules/Commands/Queries folders. Updated §3 dependency diagram: added Persistence→Application, Infrastructure→Application (interface implementation). Added TD-1 tracking note. |
