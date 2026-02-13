# MarcoERP – Versioning Policy

**Version Numbering, Release Policy, and Change Tracking**

---

## 1. Version Numbering Scheme

MarcoERP uses **Semantic Versioning (SemVer)** with phase awareness:

```
{Major}.{Minor}.{Patch}-{Phase}
```

| Segment  | Meaning                                                      | Example    |
|----------|--------------------------------------------------------------|------------|
| Major    | Breaking changes, major architecture shifts                  | `2.0.0`    |
| Minor    | New features, new modules, non-breaking additions            | `1.3.0`    |
| Patch    | Bug fixes, minor corrections, documentation updates          | `1.3.2`    |
| Phase    | Development phase tag (for pre-release tracking)             | `1.0.0-P2` |

### Current Version

```
0.1.0-P1 (Phase 1: Foundation & Governance)
```

---

## 2. Phase Versioning

| Phase | Version Range | Description                         |
|-------|---------------|-------------------------------------|
| P1    | `0.1.x`       | Foundation & Governance             |
| P2    | `0.2.x`       | Core Accounting Engine              |
| P3    | `0.3.x`       | Inventory & Warehousing             |
| P4    | `0.4.x`       | Sales & Purchasing                  |
| P5    | `0.5.x`       | Reporting & Dashboards              |
| P6    | `0.6.x`       | API & Mobile Extension              |
| GA    | `1.0.0`       | General Availability (Production)   |

---

## 3. Version Increment Rules

| Change Type                         | Version Increment | Example               |
|-------------------------------------|-------------------|-----------------------|
| Governance document update          | Patch             | `0.1.0` → `0.1.1`    |
| New entity or table                 | Minor             | `0.2.0` → `0.2.1`    |
| New module (Inventory, Sales)       | Minor             | `0.2.x` → `0.3.0`    |
| Bug fix                             | Patch             | `0.2.1` → `0.2.2`    |
| Architecture change                 | Major             | `0.x.x` → `1.0.0`    |
| Breaking API change (future)        | Major             | `1.x.x` → `2.0.0`    |
| New feature within existing module  | Minor             | `0.2.3` → `0.2.4`    |

---

## 4. Governance Document Versioning

Each governance document has its own internal version table:

```markdown
## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
```

### Rules for Governance Changes

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| GOV-01  | Every governance change includes a version history entry.                |
| GOV-02  | Changes require explicit justification in version history.               |
| GOV-03  | Governance changes increment the system Patch version.                   |
| GOV-04  | Major governance changes (architecture shifts) require Major increment.  |
| GOV-05  | All governance documents are reviewed at each phase transition.          |

---

## 5. Release Process

### 5.1 Development Releases

| Step | Action                                                                  |
|------|-------------------------------------------------------------------------|
| 1    | Development on feature branch                                           |
| 2    | All tests pass                                                          |
| 3    | Code review (or self-review with governance checklist)                  |
| 4    | Merge to main branch                                                    |
| 5    | Version tag applied                                                     |

### 5.2 Phase Releases

| Step | Action                                                                  |
|------|-------------------------------------------------------------------------|
| 1    | All features for the phase are complete                                 |
| 2    | All governance documents updated for the phase                          |
| 3    | Full regression test suite passes                                       |
| 4    | Phase release notes written                                             |
| 5    | Version tagged with phase marker (e.g., `v0.2.0-P2`)                   |
| 6    | Governance review for next phase                                        |

### 5.3 Production Releases

| Step | Action                                                                  |
|------|-------------------------------------------------------------------------|
| 1    | All phases complete                                                     |
| 2    | Full system test in staging environment                                 |
| 3    | Database backup before deployment                                       |
| 4    | Migration script reviewed                                               |
| 5    | Deployment to production                                                |
| 6    | Post-deployment verification                                            |
| 7    | Version tagged as `v1.0.0`                                              |

---

## 6. Changelog Format

A `CHANGELOG.md` file is maintained at the repository root:

```markdown
# Changelog

## [0.1.0-P1] - 2026-02-08

### Added
- Foundation & Governance documents
- Solution structure definition
- Architecture contract

### Changed
- (none)

### Fixed
- (none)

### Removed
- (none)
```

### Changelog Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| CHL-01  | Every version increment updates the CHANGELOG.md.                        |
| CHL-02  | Entries grouped by: Added, Changed, Fixed, Removed.                      |
| CHL-03  | Entries written in past tense, concise, factual.                         |
| CHL-04  | Reference related governance docs or issue numbers.                      |

---

## 7. Git Tag Convention

```
v{Major}.{Minor}.{Patch}[-{Phase}]
```

Examples:
- `v0.1.0-P1` — Phase 1 initial release
- `v0.2.0-P2` — Phase 2 initial release
- `v0.2.3-P2` — Phase 2 bug fix
- `v1.0.0` — General availability

---

## 8. Branch Strategy

| Branch Pattern                    | Purpose                              |
|-----------------------------------|--------------------------------------|
| `main`                            | Stable, released code                |
| `develop`                         | Integration branch for current phase |
| `feature/{module}/{description}`  | New feature development              |
| `fix/{description}`               | Bug fix                              |
| `governance/{document}`           | Governance document changes          |

---

## 9. Compatibility Policy

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| CMP-01  | Database migrations must be forward-compatible within a phase.           |
| CMP-02  | No breaking schema changes mid-phase without migration path.             |
| CMP-03  | Application settings format must be backward-compatible.                 |
| CMP-04  | Future API endpoints will follow versioned URL patterns (`/api/v1/`).    |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
