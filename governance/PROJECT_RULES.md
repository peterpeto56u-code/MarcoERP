# MarcoERP – Project Rules

**Master Rules Governing All Development**

---

## 1. Document Authority

This document is the **supreme governance document** for the MarcoERP project. All other governance documents derive their authority from this one. In case of conflict between any other document and PROJECT_RULES.md, this document takes precedence.

**Last updated:** Phase 1 – Foundation & Governance

---

## 2. System Identity Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| SYS-01  | The system name is **MarcoERP**. All namespaces, titles, and references must use this exact name. |
| SYS-02  | MarcoERP is a **production system**, not a prototype. Every decision must be production-grade. |
| SYS-03  | The system operates with a **single company** currently, but is architecturally prepared for **Multi-Company** support. |
| SYS-03a | A `DefaultCompany` (Id=1) is seeded on first run. All existing data belongs to this company. |
| SYS-04  | Only **one fiscal year** may be active (open) at any time.                 |
| SYS-05  | The system uses **full double-entry accounting**. No single-entry shortcuts are permitted. |
| SYS-06  | The system is designed for **Windows Desktop (WPF)** with future API extensibility. |

---

## 3. Development Discipline Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| DEV-01  | **No code without governance.** Every feature must comply with ARCHITECTURE.md. |
| DEV-02  | **No feature without a plan.** Every new module requires documented purpose, affected layers, and entity list before implementation. |
| DEV-03  | **No shortcut implementations.** "We'll fix it later" is not an acceptable justification. |
| DEV-04  | **No direct database queries from UI.** All data access goes through Application → Domain → Persistence. |
| DEV-05  | **No static mutable state** anywhere in the application. |
| DEV-06  | **No God classes.** Classes should target max 500 lines (guideline). Classes exceeding 800 lines require refactoring justification. |
| DEV-06a | 500-line target is a maintainability guideline, not an absolute limit. |
| DEV-06b | Classes between 500-800 lines: acceptable with clear single responsibility. |
| DEV-06c | Classes exceeding 800 lines: must document justification or refactor. |
| DEV-07  | **No God windows.** Each window serves one clear functional purpose. |
| DEV-08  | **No magic strings.** All constants must be defined in named constant classes or enums. |
| DEV-09  | **No hardcoded connection strings.** All configuration is externalized. |
| DEV-10  | **No suppressed exceptions.** Every catch block must log or re-throw. |
| DEV-11  | **Naming consistency is mandatory.** See naming conventions in SOLUTION_STRUCTURE.md. |
| DEV-12  | **All public methods must have XML documentation.** |
| DEV-13  | **No commented-out code in production branches.** Remove it or track it in version control. |
| DEV-14  | **No `DateTime.Now` or `DateTime.UtcNow` in business code.** Use `IDateTimeProvider`. |
| DEV-15  | **No arithmetic or business calculations in ViewModels or UI code.** All math (totals, discounts, VAT, unit conversions, profit) must be delegated to `ILineCalculationService` or equivalent Application-layer service. ViewModels may only call service methods and bind results. (Phase 9) |

---

## 4. Architectural Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| ARC-01  | Clean Architecture is mandatory. See ARCHITECTURE.md for full contract.    |
| ARC-02  | All dependencies point inward — toward the Domain.                         |
| ARC-03  | Domain layer has **zero external dependencies**.                           |
| ARC-04  | Interfaces are defined where they are **needed**, and implemented where they **belong**. |
| ARC-05  | Dependency injection is the only mechanism for cross-layer communication.  |
| ARC-06  | The composition root is exclusively in `MarcoERP.WpfUI/App.xaml.cs`.       |
| ARC-07  | No circular project references. Build order must always be deterministic.  |
| ARC-08  | Each layer can only communicate with its allowed neighbors per ARCHITECTURE.md. |
| ARC-09  | Any new project added to the solution requires an update to SOLUTION_STRUCTURE.md. |

---

## 5. Financial Integrity Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| FIN-01  | Every financial transaction must produce balanced debit and credit entries. |
| FIN-02  | Posted transactions **cannot** be edited or deleted.                       |
| FIN-03  | Corrections are made only via reversal or adjustment entries.              |
| FIN-04  | All amounts use **decimal** type — never float or double.                  |
| FIN-05  | VAT calculation is centralized in `ILineCalculationService` (Application layer). Never in UI. |
| FIN-06  | Inventory movements must generate corresponding accounting entries.        |
| FIN-07  | Period lock prevents posting to a closed period.                           |
| FIN-08  | Fiscal year close is irreversible once completed.                          |
| FIN-09  | Auto-generated codes are sequential and unique per fiscal year. Gaps may occur on transaction failures.|
| FIN-10  | Re-numbering of codes is allowed **only** for draft documents.             |
| FIN-11  | Every financial amount must be rounded to 2 decimal places (configurable). |
| FIN-12  | Balance verification must pass before any posting operation completes.     |

---

## 6. Data Integrity Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| DAT-01  | **No hard delete** for any financial or transactional data.                |
| DAT-02  | Soft delete uses `IsDeleted` flag + `DeletedAt` timestamp.                 |
| DAT-03  | Master data (Chart of Accounts, Warehouses) may only be deactivated, never physically deleted. |
| DAT-04  | **Audit logging is mandatory** for all create, update, and delete operations. |
| DAT-05  | Concurrency control via `RowVersion` column on all editable entities.      |
| DAT-06  | All timestamps are stored in UTC.                                          |
| DAT-07  | Referential integrity is enforced at both domain and database level.       |
| DAT-08  | Cascade deletes are **forbidden**. Explicit handling is required.          |

---

## 7. Testing Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| TST-01  | Domain logic must have unit test coverage before being considered complete.|
| TST-02  | Application services must have unit tests with mocked repositories.       |
| TST-03  | Financial calculations (VAT, totals, balancing) require dedicated test suites. |
| TST-04  | Posting workflows must be tested end-to-end (integration tests).          |
| TST-05  | Test names follow pattern: `MethodName_Scenario_ExpectedResult`.          |
| TST-06  | No test may depend on external state or execution order.                  |

---

## 8. Version Control Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| VCS-01  | All governance documents are version-controlled alongside source code.    |
| VCS-02  | Governance document changes require explicit justification in commit message. |
| VCS-03  | No force-push to main/production branches.                                |
| VCS-04  | Feature branches follow pattern: `feature/{module}/{short-description}`.  |
| VCS-05  | Every commit message must reference the affected module or rule.          |

---

## 9. Error Handling Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| ERR-01  | Domain throws domain-specific exceptions (e.g., `UnbalancedEntryException`). |
| ERR-02  | Application catches domain exceptions and translates to user-facing DTOs. |
| ERR-03  | WpfUI displays user-friendly messages; never raw exception text.          |
| ERR-04  | Persistence exceptions are caught and wrapped in application exceptions.  |
| ERR-05  | All unhandled exceptions are logged with full stack trace.                |
| ERR-06  | Validation errors are returned as structured results, not thrown exceptions. |

---

## 10. Performance Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| PRF-01  | No lazy loading in Entity Framework. All loading is explicit (`Include`).  |
| PRF-02  | Large datasets must use pagination. No unbounded queries.                  |
| PRF-03  | UI windows must not block the main thread for data operations.            |
| PRF-04  | Database indexes must be defined for all foreign keys and frequent query columns. |
| PRF-05  | Batch operations must be used for bulk inserts and updates.               |

---

## 11. Documentation Rules

| Rule ID | Rule                                                                       |
|---------|----------------------------------------------------------------------------|
| DOC-01  | Every governance document has a clear version history section.            |
| DOC-02  | New modules require a brief design document before implementation.        |
| DOC-03  | API-ready interfaces must have complete XML documentation.                |
| DOC-04  | Complex business rules must have accompanying explanation comments.       |
| DOC-05  | Database schema changes must be documented in DATABASE_POLICY.md.         |

---

## 12. Rule Enforcement

- **Before any new feature:** Check PROJECT_RULES.md and ARCHITECTURE.md
- **Before any new entity:** Check ACCOUNTING_PRINCIPLES.md and DATABASE_POLICY.md
- **Before any UI window:** Check UI_GUIDELINES.md
- **Before any governance change:** Justify and log in VERSIONING.md
- **Before AI agent acts:** Check AGENT_POLICY.md and AGENT_CONTROL_SYSTEM.md

**Violation of any rule in this document is a development blocker.** Work cannot proceed until the violation is resolved.

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
| 2.0     | 2026-02-12 | Company Isolation: SYS-03 updated for Multi-Company architectural preparation |
| 2.1     | 2026-02-13 | FIN-05: Updated VAT layer from Domain to Application (ILineCalculationService). |
