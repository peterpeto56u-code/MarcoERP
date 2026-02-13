MarcoERP — Production Hardening Addendum
Version 1.1 — Production-Ready Edition

Date: 9 February 2026
Status: Mandatory Addendum for Commercial Release
Applies To: Master Execution Plan v1.0

📌 Purpose

This document upgrades MarcoERP from:

A Complete Accounting System
to
A Commercial-Grade, Production-Ready ERP Platform

This addendum defines mandatory architectural layers required for:

Security

Data integrity

Performance

Disaster recovery

Scalability

Commercial deployment

1️⃣ Backup & Disaster Recovery Layer
🎯 Objective

Guarantee business continuity and eliminate risk of financial data loss.

Phase 2E.5 — Backup & Recovery
1. Automated Backups

Daily scheduled SQL Server backup

Optional backup on application close

Mandatory backup before fiscal year close

2. Manual Backup

“Create Backup” button

User-selected save path

Export as .bak

3. Restore Wizard

Select backup file

Confirmation step

Automatic application restart after restore

4. Full Data Export (JSON)

Export all entities

Import all entities

Used for migration and auditing

5. Backup Encryption

AES encryption

Password-protected backups

2️⃣ Security & Authorization Layer
🎯 Objective

Implement enterprise-grade permission management.

Phase 5C — RBAC (Role-Based Access Control)
Roles

Administrator

Accountant

Sales User

Storekeeper

Viewer

Permission Examples

Accounting.PostJournal

Accounting.CloseYear

Sales.PostInvoice

Sales.DeleteInvoice

Inventory.Adjust

Treasury.Transfer

Settings.Edit

Screen-Level Authorization

Prevent screen access without permission

Hide sidebar menu items dynamically

Action-Level Authorization

Hide Post button if no permission

Hide Delete button if no permission

Validate permission again in Application Layer

Login Security

BCrypt password hashing

Lock account after 5 failed attempts

Store last login timestamp

Force password change on first login

Audit Trail Viewer

Filter by user

Filter by date

Export activity log

Track: create / edit / post / delete / login

3️⃣ System Configuration Layer
🎯 Objective

Remove hardcoded system accounts and increase flexibility.

Phase 5D — System Settings Table
Core Settings
Setting	Description
DefaultInventoryAccountId	Inventory control account
DefaultRevenueAccountId	Sales revenue account
DefaultVatInputAccountId	VAT input
DefaultVatOutputAccountId	VAT output
DefaultCashboxId	Default treasury
InvoiceNumberFormat	Example: PI-YYYYMM-####
CurrencySymbol	EGP / USD
CompanyName	Business name
CompanyAddress	Business address
FinancialPrecision	Default: 2
CostPrecision	Default: 4
4️⃣ Performance & Scalability Hardening
🎯 Objective

Ensure system stability under large datasets.

Phase 2D.5 — Performance Strategy
Index Strategy

Composite index (FiscalYearId, JournalNumber)

Index on JournalDate

Index on (ProductId, WarehouseId)

Index on InvoiceDate

Index on AccountId

Pagination

Mandatory paging on all listing screens

Default page size = 50

Dashboard Caching

Memory cache duration: 60 seconds

Auto-refresh on posting operations

Read Projections

Lightweight reporting queries

No aggregate loading for reports

Use AsNoTracking for read-only queries

5️⃣ Background Jobs & Automation
🎯 Objective

Automate operational and financial routines.

Phase 5E — Background Services
Daily Summary Job

Daily sales total

Daily purchases total

Treasury movement summary

Aging Recalculation Job

Recalculate receivables aging

Recalculate payables aging

Integrity Verification Job

Detect unbalanced journals

Detect stock mismatch

Validate WAC consistency

Automated Backup Job

Run daily at 02:00 AM

Log backup result

6️⃣ Data Integrity & Internal Audit Tools
🎯 Objective

Maintain accounting integrity long-term.

Phase 5F — System Health & Audit Tools
Trial Balance Rebuild Tool

Recalculate balances from journal entries.

WAC Rebuild Tool

Recalculate weighted average cost from inventory movements.

Stock Reconciliation Tool

Compare:

WarehouseProduct quantity
vs

InventoryMovement totals

Orphan Detection Tool

Detect:

Inventory movements without source

Journal entries without source reference

Broken foreign keys

Database Health Report

Display:

Table sizes

Record counts

Last backup date

Last fiscal close date

7️⃣ Production Deployment Checklist

Before releasing MarcoERP:

 Backup automation verified

 Restore process tested

 RBAC fully implemented

 Audit logging active

 Serializable transactions enforced

 Performance indexes added

 Trial balance verified

 WAC precision tested

 Multi-user testing completed

 Database health report clean

8️⃣ Definition of Production Readiness

MarcoERP is Production-Ready when:

No posted journal can be edited.

No journal can be unbalanced.

No sale can occur without stock.

No posting outside open fiscal period.

No data loss possible without recovery.

All operations logged.

All permissions enforced at UI and Application levels.

Updated Execution Flow
Core Build (2A → 4C)
        ↓
Security Layer (5C)
        ↓
System Settings (5D)
        ↓
Performance Hardening (2D.5)
        ↓
Background Jobs (5E)
        ↓
Integrity Tools (5F)
        ↓
Backup Layer (2E.5)
        ↓
Final QA & Load Testing
        ↓
🚀 Production Release

Final Status

MarcoERP Execution Plan — Version 1.1
Production-Ready Architecture