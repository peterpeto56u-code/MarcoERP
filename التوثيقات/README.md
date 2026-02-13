# MarcoERP

**Medium ERP – Full Double Entry Accounting System**

---

## Overview

MarcoERP is a production-grade, medium-scale Enterprise Resource Planning system built for long-term stability. It provides full double-entry accounting, inventory management, multi-warehouse support, and financial governance — all within a structured, auditable architecture.

This is **not** a prototype. Every architectural decision is made for durability, correctness, and maintainability over years of production use.

---

## System Identity

| Property              | Value                                  |
|-----------------------|----------------------------------------|
| System Name           | MarcoERP                               |
| System Type           | Medium ERP – Full Double Entry         |
| Platform              | Windows Desktop (WinForms)             |
| Database              | SQL Server Express                     |
| ORM                   | Entity Framework Core                  |
| Target Lifecycle      | Long-term production (5+ years)        |
| Current Phase         | Phase 1 – Foundation & Governance      |

---

## Core Business Characteristics

- **Single Company** – One company per installation
- **One Active Fiscal Year** – Only one fiscal year open at a time
- **Monthly Period Lock** – Periods are locked individually per month
- **VAT Enabled** – VAT is active by default on all relevant transactions
- **Multi-Warehouse** – Each warehouse has its own inventory ledger
- **Multi-Cashbox** – Multiple cashboxes supported
- **Cost Centers** – Optional tagging for cost center reporting
- **Draft Invoices** – All invoices begin as drafts before posting
- **Auto-Generated Codes** – For invoices, products, customers, journals
- **No Edit After Posting** – Posted transactions are immutable
- **Mandatory Audit Log** – Every data mutation is logged
- **Re-numbering Before Posting Only** – Draft codes may be adjusted
- **Future Extensibility** – Designed for API and Mobile integration

---

## Phase Roadmap

| Phase | Name                        | Status      |
|-------|-----------------------------|-------------|
| 1     | Foundation & Governance     | **Active**  |
| 2     | Core Accounting Engine      | Planned     |
| 3     | Inventory & Warehousing     | Planned     |
| 4     | Sales & Purchasing          | Planned     |
| 5     | Reporting & Dashboards      | Planned     |
| 6     | API & Mobile Extension      | Planned     |

---

## Phase 1 Status: LOCKED ✅

**Phase 1 (Foundation & Governance) is COMPLETE and LOCKED.**

- **Version:** 0.1.1-P1-LOCKED
- **Lock Date:** February 8, 2026
- **Status:** Production-ready governance framework
- **Compliance:** 11/11 locked business decisions implemented (100%)
- **Internal Consistency:** Zero contradictions or ambiguities

### Key Documents

- [PHASE1_COMPLETION.md](PHASE1_COMPLETION.md) — Deliverables summary and assumptions
- [PHASE1_AUDIT_REPORT.md](PHASE1_AUDIT_REPORT.md) — Comprehensive governance audit
- [PHASE1_CORRECTION_CHECKLIST.md](PHASE1_CORRECTION_CHECKLIST.md) — Applied corrections
- [PHASE1_LOCK_CONFIRMATION.md](PHASE1_LOCK_CONFIRMATION.md) — Final consistency validation ✅

**Phase 2 (Core Accounting Engine) may now proceed.**

---

## Governance Documents

All governance documents are located in the `/governance/` directory:

| Document                        | Purpose                                         |
|---------------------------------|-------------------------------------------------|
| PROJECT_RULES.md                | Master rules governing all development           |
| ARCHITECTURE.md                 | Layer responsibilities and contracts             |
| DATABASE_POLICY.md              | Database design and access rules                 |
| UI_GUIDELINES.md                | WinForms UI standards and patterns               |
| AGENT_POLICY.md                 | AI agent behavior control and boundaries         |
| VERSIONING.md                   | Version numbering and release policy             |
| SECURITY_POLICY.md              | Authentication, authorization, and data safety   |
| ACCOUNTING_PRINCIPLES.md        | Financial engine rules and double-entry logic    |
| FINANCIAL_ENGINE_RULES.md       | Posting, journals, fiscal year, period lock      |
| RECORD_PROTECTION_POLICY.md     | Immutability, reversals, adjustments             |
| RISK_PREVENTION_FRAMEWORK.md    | Structural failure prevention                    |
| AGENT_CONTROL_SYSTEM.md         | AI agent permissions and validation checklists   |

---

## Solution Structure

See [SOLUTION_STRUCTURE.md](governance/SOLUTION_STRUCTURE.md) for the full .NET solution layout, dependency rules, and forbidden cross-layer access.

---

## Phase 1 Status

✅ **Phase 1 (Foundation & Governance) is COMPLETE.**

See [PHASE1_COMPLETION.md](PHASE1_COMPLETION.md) for:
- Full deliverables summary
- Business assumptions made
- Clarification questions before Phase 2
- Recommendations for next steps

---

## License

Proprietary. All rights reserved.
