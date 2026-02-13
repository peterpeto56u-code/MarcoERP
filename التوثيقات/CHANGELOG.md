# Changelog

All notable changes to the MarcoERP project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.1.2-P1] - 2026-02-11

### Changed

- Updated UI guidelines to allow shell-based `{Entity}{Type}View` naming alongside window naming.
- Strengthened unsaved-changes guard to require Save/Discard/Cancel across navigation and window close.

---

## [0.1.1-P1-LOCKED] - 2026-02-08

### Changed

**Phase 1 Governance: Final Corrections and Lock**

This version applies all corrections identified during the Phase 1 architectural audit. All governance documents are now aligned with the 11 locked business decisions. Phase 1 governance is now LOCKED and ready for Phase 2 implementation.

**Locked Business Decisions Implemented:**
1. Fiscal year is calendar-based (Jan 1 – Dec 31)
2. Only ONE fiscal year can be active at a time
3. Closed fiscal years CANNOT be reopened
4. Closed accounting periods CANNOT be reopened
5. Reversal entries allowed ONLY if original is in OPEN period
6. Closed period corrections via Adjustment Entry only
7. Adjustment Entries must reference original transaction
8. Sequential & unique codes (gaps allowed on failure)
9. Inventory costing: Weighted Average ONLY (no FIFO)
10. 500-line rule is guideline, not strict limit
11. Multi-user ready from Phase 2 (optimistic concurrency)

**Documents Modified:**

- **PROJECT_RULES.md (v0.1.1)**
  - DEV-06: Softened 500-line rule from strict limit to guideline (500 target, 800 limit)
  - Added DEV-06a, DEV-06b, DEV-06c for clarification
  - FIN-09: Removed "gap-free" terminology, changed to "sequential and unique"

- **FINANCIAL_ENGINE_RULES.md (v0.1.1)**
  - Step 11: Removed "gap-free" from journal numbering
  - JNL-06: Changed to "sequential and unique" with gaps allowed
  - Added FY-00, FY-01: Explicit calendar year definition (Jan 1 – Dec 31)
  - Renumbered existing FY-01 through FY-10 as FY-02 through FY-11

- **RECORD_PROTECTION_POLICY.md (v0.1.1)**
  - REV-07: **CRITICAL FIX** — Changed reversal date from "current date" to "original transaction date"
  - Added REV-13, REV-14, REV-15: Period lock enforcement for reversals
  - Added Section 3.5: Reversal and Period Lock Interaction (3 scenarios)
  - Added REV-P1 through REV-P5: Period lock policy rules
  - Added ADJ-08 through ADJ-11: Adjustment mechanism for closed period corrections
  - Updated Section 4.3: Enhanced Reversal vs. Adjustment Decision Guide
  - ACG-04: Removed "gap-free", changed to "sequential and unique"
  - SEQ-04 through SEQ-07: **MAJOR REWRITE** of sequence management rules
    - Sequence numbers consumed even on failure (gaps allowed)
    - SQL Server SEQUENCE object for multi-user uniqueness
    - Gaps are normal and do not indicate corruption

- **ACCOUNTING_PRINCIPLES.md (v0.1.1)**
  - Fiscal Year: Added explicit "Calendar year: January 1 to December 31 (always)"
  - INV-02: Removed FIFO support, changed to "Weighted Average Cost (ONLY)"
  - Added INV-06 through INV-09: Weighted Average precision rules
    - Cost per unit: 4 decimal places
    - Financial amounts: 2 decimal places
    - Banker's Rounding after each receipt
    - Extended cost rounded to 2 decimals

- **PHASE1_COMPLETION.md (v0.1.1)**
  - T4: Changed from "single-user initially" to "multi-user ready from the start" (✅ CONFIRMED)
  - Q1, Q2, Q3, Q6: Marked as **✅ RESOLVED** with locked decisions

### Added

- **PHASE1_AUDIT_REPORT.md (v1.0)** — Comprehensive audit of Phase 1 governance against locked decisions
- **PHASE1_CORRECTION_CHECKLIST.md (v1.0)** — Quick reference for applying all corrections
- **PHASE1_LOCK_CONFIRMATION.md (v1.0)** — Final consistency check and production readiness declaration

### Fixed

- **Reversal date contradiction:** Resolved conflict between "current date" and "original transaction date"
- **Gap-free terminology:** Eliminated 4+ instances contradicting multi-user reality
- **Fiscal year ambiguity:** Made calendar year (Jan-Dec) explicit
- **FIFO support ambiguity:** Removed FIFO, clarified Weighted Average only
- **500-line rule rigidity:** Softened from mandatory to guideline
- **Multi-user assumption:** Confirmed multi-user ready from start
- **Reversal + period lock interaction:** Defined clear policy (no bypass, use adjustment for closed periods)
- **Sequence consumption:** Clarified that gaps occur on failure (normal behavior)

### Governance Statistics

- **Total governance documents:** 19
- **Documents modified in this version:** 5
- **Documents already compliant:** 14
- **Total corrections applied:** 28
- **New rules added:** 25
- **Contradictions eliminated:** 6
- **Ambiguities resolved:** 5
- **Compliance rate:** 100%

### Status

**✅ Phase 1 Governance is now internally consistent and locked.**

Phase 2 (Core Accounting Engine) may proceed with confidence that governance is stable and will not change mid-development.

---

## [0.1.0-P1] - 2026-02-08

### Added

**Phase 1: Foundation & Governance**

- README.md — Project overview and identity
- SOLUTION_STRUCTURE.md — Complete solution layout, dependency rules, forbidden cross-layer access
- ARCHITECTURE.md — Layer responsibilities and explicit prohibitions
- PROJECT_RULES.md — Master rules governing all development
- DATABASE_POLICY.md — Database design, access rules, and schema governance
- UI_GUIDELINES.md — WinForms UI standards, patterns, and constraints
- AGENT_POLICY.md — AI agent behavior control and boundaries
- VERSIONING.md — Version numbering and release policy
- SECURITY_POLICY.md — Authentication, authorization, and data safety
- ACCOUNTING_PRINCIPLES.md — Double-entry enforcement and accounting logic
- FINANCIAL_ENGINE_RULES.md — Posting workflow, journal integrity, fiscal year/period lock
- RECORD_PROTECTION_POLICY.md — Immutability, reversals, adjustments, auto-code generation
- RISK_PREVENTION_FRAMEWORK.md — Structural failure prevention and long-term maintainability
- AGENT_CONTROL_SYSTEM.md — AI agent permissions, validation checklists, impact analysis
- CHANGELOG.md — This file

### Changed

- (none)

### Fixed

- (none)

### Removed

- (none)

---

## [Unreleased]

### Planned for Phase 2

- Core accounting engine implementation
- Domain entities for Chart of Accounts, Journal Entries, Fiscal Year/Period
- Application services for posting workflow
- Entity Framework Core DbContext and configurations
- Basic UI forms for Chart of Accounts and Journal Entry
