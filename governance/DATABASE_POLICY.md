# MarcoERP – Database Policy

**Database Design, Access Rules, and Schema Governance**

---

## 1. Database Platform

| Property             | Value                                      |
|----------------------|--------------------------------------------|
| Engine               | Microsoft SQL Server Express               |
| ORM                  | Entity Framework Core                      |
| Migration Strategy   | Code-First with explicit Fluent API        |
| Naming Convention    | PascalCase for tables, columns, and keys   |
| Schema               | `dbo` (default), module-specific schemas later if needed |

---

## 2. General Database Rules

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| DB-01   | **Code-First only.** No manual table creation outside of EF migrations.  |
| DB-02   | **Fluent API only.** No data annotations on domain entities.              |
| DB-03   | **No lazy loading.** All navigation properties loaded explicitly.         |
| DB-04   | **No cascade delete.** All delete behavior is `Restrict` or `NoAction`.  |
| DB-05   | **No dynamic SQL** unless formally approved and documented.               |
| DB-06   | **No direct DbContext usage** outside of Persistence layer.               |
| DB-07   | **All tables have a primary key.** Preferably `int` identity or `Guid`.  |
| DB-08   | **All foreign keys are indexed.** EF Core does not auto-index FKs.       |
| DB-09   | **All money columns use `decimal(18,4)`** for calculation precision. |
| DB-10   | **All string columns have explicit `MaxLength`.**                         |
| DB-11   | **No `ntext`, `text`, or `image` columns.** Use `nvarchar(max)` or `varbinary(max)`. |
| DB-12   | **UTC timestamps only.** Use `datetime2` for all date/time columns.      |

---

## 3. Table Naming Conventions

| Element              | Convention                      | Example                        |
|----------------------|---------------------------------|--------------------------------|
| Table                | PascalCase, plural              | `JournalEntries`               |
| Column               | PascalCase                      | `AccountCode`                  |
| Primary Key          | `Id`                            | `Id`                           |
| Foreign Key Column   | `{RelatedEntity}Id`             | `AccountId`                    |
| Foreign Key Constraint | `FK_{Table}_{RelatedTable}`   | `FK_JournalEntries_Accounts`   |
| Index                | `IX_{Table}_{Column(s)}`        | `IX_JournalEntries_PostingDate`|
| Unique Constraint    | `UQ_{Table}_{Column(s)}`        | `UQ_Accounts_AccountCode`      |
| Check Constraint     | `CK_{Table}_{Rule}`             | `CK_JournalEntries_Balance`    |

---

## 4. Mandatory Columns

### 4.1 Base Entity Columns (Every Table)

| Column         | Type            | Nullable | Purpose                           |
|----------------|-----------------|----------|-----------------------------------|
| `Id`           | `int` IDENTITY  | No       | Primary key                       |
| `CreatedAt`    | `datetime2`     | No       | UTC creation timestamp            |
| `CreatedBy`    | `nvarchar(100)` | No       | User who created the record       |
| `ModifiedAt`   | `datetime2`     | Yes      | UTC last modification timestamp   |
| `ModifiedBy`   | `nvarchar(100)` | Yes      | User who last modified the record |
| `RowVersion`   | `rowversion`    | No       | Optimistic concurrency token      |

### 4.2 Soft Delete Columns (Financial & Master Data Tables)

| Column         | Type            | Nullable | Purpose                           |
|----------------|-----------------|----------|-----------------------------------|
| `IsDeleted`    | `bit`           | No       | Soft delete flag (default: 0)     |
| `DeletedAt`    | `datetime2`     | Yes      | UTC deletion timestamp            |
| `DeletedBy`    | `nvarchar(100)` | Yes      | User who performed soft delete    |

### 4.3 Status Columns (Transactional Tables)

| Column         | Type            | Nullable | Purpose                           |
|----------------|-----------------|----------|-----------------------------------|
| `Status`       | `int`           | No       | Enum: Draft, Posted, Reversed     |
| `PostedAt`     | `datetime2`     | Yes      | UTC posting timestamp             |
| `PostedBy`     | `nvarchar(100)` | Yes      | User who posted the record        |

---

## 5. Primary Key Strategy

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| PK-01   | Use `int` IDENTITY (1,1) as primary key for all tables by default.       |
| PK-02   | `Guid` primary key only when required for future API/sync scenarios.     |
| PK-03   | No composite primary keys on main tables. Use unique indexes instead.   |
| PK-04   | Junction tables (many-to-many) may use composite keys of the two FKs.  |

---

## 6. Migration Policy

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| MIG-01  | Every schema change goes through an EF Core migration.                   |
| MIG-02  | Migration names follow: `{YYYYMMDD}_{SequenceNumber}_{Description}`.     |
| MIG-03  | Example: `20260208_001_CreateAccountsTable`.                              |
| MIG-04  | Migrations must be reviewed before application to production.            |
| MIG-05  | **No data loss migrations** in production. Columns added are nullable or have defaults. |
| MIG-06  | Down migrations must be functional — every `Up()` must have a valid `Down()`. |
| MIG-07  | Seed data is applied through dedicated seed classes, not inside migrations. |
| MIG-08  | Production migrations run through a controlled deployment process.       |

---

## 7. Index Policy

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| IDX-01  | All foreign key columns must have an index.                              |
| IDX-02  | Columns used in `WHERE`, `ORDER BY`, or `JOIN` frequently must be indexed. |
| IDX-03  | Composite indexes: place most selective column first.                    |
| IDX-04  | No redundant indexes. Review before adding.                              |
| IDX-05  | Unique indexes for business-unique fields (AccountCode, InvoiceNumber).  |
| IDX-06  | Filtered indexes for soft-delete queries: `WHERE IsDeleted = 0`.         |

---

## 8. Audit Log Table Design

The audit log captures all data mutations (Create, Update, SoftDelete):

| Column          | Type              | Purpose                                 |
|-----------------|-------------------|-----------------------------------------|
| `Id`            | `bigint` IDENTITY | Primary key (high-volume table)         |
| `EntityType`    | `nvarchar(200)`   | Name of the affected entity/table       |
| `EntityId`      | `int`             | Primary key value of affected record    |
| `Action`        | `nvarchar(50)`    | `Created`, `Updated`, `SoftDeleted`     |
| `PerformedBy`   | `nvarchar(100)`   | User who made the change                |
| `Details`       | `nvarchar(500)`   | Human-readable description              |
| `OldValues`     | `nvarchar(max)`   | JSON of previous values (null on Create)|
| `NewValues`     | `nvarchar(max)`   | JSON of new values                      |
| `ChangedColumns`| `nvarchar(max)`   | JSON array of changed column names      |
| `Timestamp`     | `datetime2`       | UTC timestamp of the change             |

> **Note:** `IpAddress` column is omitted — MarcoERP is a desktop application where client IP is not meaningful. If a future API layer is added, `IpAddress` should be introduced at that time.

### Audit Log Rules

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| AUD-01  | Audit log is **append-only**. No updates or deletes on audit records.    |
| AUD-02  | Audit log is populated via EF Core `SaveChanges` interceptor.            |
| AUD-03  | Audit log covers all tables except the audit log itself.                 |
| AUD-04  | Performance: audit writes must not block the main transaction excessively.|
| AUD-05  | Audit data retention policy: minimum 7 years for financial records.      |

---

## 9. Connection Management

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| CON-01  | Connection string stored in application configuration (not hardcoded).   |
| CON-02  | Connection pooling enabled (default SQL Server behavior).                |
| CON-03  | Timeout configuration: 30 seconds for queries, 60 seconds for reports.   |
| CON-04  | DbContext lifetime: scoped per operation (not singleton).                |
| CON-05  | Retry policy enabled for transient failure handling.                     |

---

## 10. Transaction Policy

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| TRX-01  | Unit of Work pattern controls transaction boundaries.                    |
| TRX-02  | Application layer initiates transactions, Persistence layer executes them.|
| TRX-03  | Posting operations must be atomic — all or nothing.                      |
| TRX-04  | Read operations do not require explicit transactions.                    |
| TRX-05  | Isolation level: `Read Committed` by default. `Serializable` for posting.|
| TRX-06  | No nested transactions. One scope per operation.                         |

---

## 11. Backup & Recovery

| Rule ID | Rule                                                                      |
|---------|---------------------------------------------------------------------------|
| BKP-01  | Database backup strategy is documented and automated.                    |
| BKP-02  | Full backup before any migration in production.                          |
| BKP-03  | Transaction log backups for point-in-time recovery.                      |
| BKP-04  | Restore process tested quarterly.                                        |

---

## 12. Forbidden Practices

| #  | Forbidden Practice                                                        |
|----|---------------------------------------------------------------------------|
| 1  | Using `SELECT *` in any query or EF projection                           |
| 2  | Having SQL logic in Application or UI layers                              |
| 3  | Using stored procedures for business logic (data access only)            |
| 4  | Allowing null in financial amount columns                                 |
| 5  | Using `float` or `real` for monetary values                              |
| 6  | Creating tables without a primary key                                    |
| 7  | Using triggers for business logic (audit interceptor only)               |
| 8  | Disabling foreign key constraints for "performance"                      |
| 9  | Using `TRUNCATE` on any production table                                  |
| 10 | Sharing DbContext instances across threads                                |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
| 1.1     | 2026-02-13 | Updated §8 AuditLog table design to match actual implementation (EntityType/EntityId/PerformedBy/Details columns, removed IpAddress). |
