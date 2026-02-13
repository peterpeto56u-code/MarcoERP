# MarcoERP – Security Policy

**Authentication, Authorization, Data Safety, and Access Control**

---

## 1. Security Model Overview

MarcoERP implements a **role-based access control (RBAC)** model with the following security layers:

```
┌──────────────────────────────┐
│     Authentication           │  Who are you?
├──────────────────────────────┤
│     Authorization            │  What can you do?
├──────────────────────────────┤
│     Data Access Control      │  What can you see?
├──────────────────────────────┤
│     Audit Trail              │  What did you do?
├──────────────────────────────┤
│     Data Protection          │  How is data secured?
└──────────────────────────────┘
```

---

## 2. Authentication

### 2.1 Authentication Method

| Property           | Value                                           |
|--------------------|-------------------------------------------------|
| Method             | Local application authentication                 |
| Storage            | User credentials stored in MarcoERP database    |
| Password Hashing   | BCrypt or PBKDF2 (with salt)                    |
| Session Type       | In-memory session per Windows session            |
| Future Option      | Windows Authentication / Active Directory        |

### 2.2 Authentication Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| AUTH-01 | Every application launch requires user login.                            |
| AUTH-02 | Passwords are **never** stored in plain text.                            |
| AUTH-03 | Password hashing uses a unique salt per user.                            |
| AUTH-04 | Minimum password length: 8 characters.                                   |
| AUTH-05 | Failed login attempts logged in audit trail.                             |
| AUTH-06 | Account lockout after 5 consecutive failed attempts (configurable).      |
| AUTH-07 | Lockout duration: 15 minutes (configurable).                             |
| AUTH-08 | Session timeout after 30 minutes of inactivity (configurable).           |
| AUTH-09 | Password change does not require restart.                                |
| AUTH-10 | First-time setup creates a default admin account.                        |

---

## 3. Authorization

### 3.1 Role-Based Access Control (RBAC)

#### Default Roles

| Role          | Description                                           | Scope          |
|---------------|-------------------------------------------------------|----------------|
| Administrator | Full system access, user management, settings          | Everything     |
| Accountant    | Full accounting access, posting, reports               | Financial      |
| Clerk         | Data entry, draft creation, no posting                 | Data Entry     |
| Viewer        | Read-only access to reports and records                | Read Only      |
| Warehouse     | Inventory operations, stock counts, transfers          | Inventory      |

#### Permission Matrix

| Permission                  | Admin | Accountant | Clerk | Viewer | Warehouse |
|-----------------------------|-------|------------|-------|--------|-----------|
| Create Draft Invoice        | Yes   | Yes        | Yes   | No     | No        |
| Post Invoice                | Yes   | Yes        | No    | No     | No        |
| Create Journal Entry        | Yes   | Yes        | No    | No     | No        |
| Post Journal Entry          | Yes   | Yes        | No    | No     | No        |
| View Reports                | Yes   | Yes        | Yes   | Yes    | No        |
| Manage Chart of Accounts    | Yes   | Yes        | No    | No     | No        |
| Close Fiscal Period         | Yes   | Yes        | No    | No     | No        |
| Close Fiscal Year           | Yes   | No         | No    | No     | No        |
| Manage Users                | Yes   | No         | No    | No     | No        |
| Manage Settings             | Yes   | No         | No    | No     | No        |
| Inventory Receipt           | Yes   | No         | No    | No     | Yes       |
| Inventory Transfer          | Yes   | No         | No    | No     | Yes       |
| Stock Count                 | Yes   | No         | No    | No     | Yes       |
| View Audit Log              | Yes   | No         | No    | No     | No        |
| Reverse Posted Transaction  | Yes   | Yes        | No    | No     | No        |

### 3.2 Authorization Rules

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| AUTHZ-01| Authorization is checked at the **Application layer** (use-case entry). |
| AUTHZ-02| UI hides or disables elements the user cannot access.                    |
| AUTHZ-03| UI hiding is a convenience — Application layer is the true enforcement.  |
| AUTHZ-04| Unauthorized access attempts are logged in the audit trail.              |
| AUTHZ-05| Role changes take effect immediately — no session restart required.       |
| AUTHZ-06| Custom roles will be supported in future phases.                         |
| AUTHZ-07| Role assignment is Admin-only.                                           |

---

## 4. Data Access Control

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| DAC-01  | Users see only data relevant to their role.                              |
| DAC-02  | Warehouse users see only their assigned warehouse(s).                    |
| DAC-03  | Financial reports filtered by authorized fiscal periods.                 |
| DAC-04  | Audit log visible only to Administrators.                                |
| DAC-05  | Sensitive fields (password hash, internal IDs) never exposed in DTOs.   |

---

## 5. Data Protection

### 5.1 At Rest

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| DPR-01  | SQL Server Transparent Data Encryption (TDE) recommended for production. |
| DPR-02  | Database backup files must be stored securely.                           |
| DPR-03  | Connection strings encrypted in application configuration.               |
| DPR-04  | No sensitive data in log files (passwords, tokens, personal data).      |

### 5.2 In Transit

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| DPT-01  | Future API connections must use HTTPS/TLS.                               |
| DPT-02  | Database connections use encrypted connections where possible.            |
| DPT-03  | No sensitive data passed as URL parameters (future API).                 |

### 5.3 In Memory

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| DPM-01  | Password strings cleared after authentication validation.                |
| DPM-02  | No caching of sensitive data in global/static variables.                 |
| DPM-03  | Session data cleared on logout.                                          |

---

## 6. Audit Trail Security

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| AUS-01  | Audit log is append-only. No mechanism to modify or delete audit records.|
| AUS-02  | Audit log includes user identity, timestamp, action, and changed data.   |
| AUS-03  | Audit log is stored in the same database but protected from application deletes. |
| AUS-04  | Audit log viewing requires Administrator role.                           |
| AUS-05  | Audit log retention: minimum 7 years for financial records.              |

---

## 7. Input Validation Security

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| INP-01  | All user input validated at UI level (format) and Application level (business). |
| INP-02  | Parameterized queries enforced (EF Core handles this by default).        |
| INP-03  | No dynamic SQL concatenation.                                            |
| INP-04  | String inputs trimmed and sanitized before processing.                   |
| INP-05  | File paths validated and restricted to allowed directories.              |
| INP-06  | Maximum input lengths enforced per DATABASE_POLICY.md column definitions.|

---

## 8. Error Handling Security

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| EHS-01  | Exception details never displayed to end users.                          |
| EHS-02  | Stack traces logged internally, user sees friendly message.              |
| EHS-03  | Authentication errors give generic messages (no "user not found" vs "wrong password"). |
| EHS-04  | Database errors wrapped — no SQL error details shown to users.           |

---

## 9. Configuration Security

| Rule ID | Rule                                                                     |
|---------|--------------------------------------------------------------------------|
| CFG-01  | Connection strings not stored in source code.                            |
| CFG-02  | Sensitive settings use DPAPI or Windows Credential Manager.              |
| CFG-03  | Default admin password must be changed at first login.                   |
| CFG-04  | Debug mode disabled in production builds.                                |
| CFG-05  | Application configuration file is not user-editable in production.       |

---

## 10. Future Security Considerations

| Feature                       | Phase     | Notes                                |
|-------------------------------|-----------|--------------------------------------|
| API Token Authentication      | Phase 6   | JWT or API Key based                 |
| Multi-factor Authentication   | Future    | For admin operations                 |
| Field-level Encryption        | Future    | For PII data                         |
| IP Whitelisting (API)         | Phase 6   | For API access                       |
| Automated Security Scanning   | Future    | Dependency vulnerability scanning    |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
