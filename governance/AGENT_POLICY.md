# MarcoERP – Agent Policy

**AI Agent Behavior Control and Boundaries**

---

## 1. Purpose

This document defines the rules and boundaries for any AI agent (including GitHub Copilot, ChatGPT, or any other LLM-based assistant) assisting in MarcoERP development. The AI agent is a **tool**, not an architect. It operates within the boundaries defined by governance documents.

---

## 2. Agent Authority Levels

| Level    | Description                                      | Requires Approval |
|----------|--------------------------------------------------|-------------------|
| EXECUTE  | Agent may perform this action autonomously        | No                |
| PROPOSE  | Agent proposes, human reviews and approves         | Yes               |
| FORBIDDEN| Agent must never perform this action              | N/A               |

---

## 3. What the Agent CAN Do (EXECUTE Level)

| #  | Allowed Action                                                          |
|----|-------------------------------------------------------------------------|
| 1  | Create new entity classes that follow existing patterns in Domain       |
| 2  | Create new DTO classes in Application layer                              |
| 3  | Create new repository implementations following established interfaces  |
| 4  | Create new EF Core configuration classes following existing conventions  |
| 5  | Create new service classes implementing defined interfaces               |
| 6  | Create new validator classes                                             |
| 7  | Write unit tests for existing services and entities                      |
| 8  | Add XML documentation to undocumented public members                    |
| 9  | Fix compilation errors that don't change business logic                 |
| 10 | Refactor code for readability without changing behavior                  |
| 11 | Create new window classes following UI_GUIDELINES.md                     |
| 12 | Add indexes to existing EF configurations                                |

---

## 4. What the Agent Must PROPOSE (Not Execute Directly)

| #  | Requires Proposal & Approval                                            |
|----|-------------------------------------------------------------------------|
| 1  | Any new database table or entity                                        |
| 2  | Any change to existing entity properties                                |
| 3  | Any new project or assembly added to the solution                       |
| 4  | Any change to dependency direction between projects                     |
| 5  | Any modification to a governance document                               |
| 6  | Any new interface definition in Domain                                   |
| 7  | Any change to posting workflow or financial rules                        |
| 8  | Any new NuGet package dependency                                         |
| 9  | Any database migration                                                   |
| 10 | Any change to the chart of accounts structure                            |
| 11 | Any modification to audit log behavior                                   |
| 12 | Any change to auto-code generation logic                                 |

---

## 5. What the Agent Must NEVER Do (FORBIDDEN)

| #  | Absolutely Forbidden                                                     |
|----|--------------------------------------------------------------------------|
| 1  | **NEVER** modify governance documents without explicit human instruction |
| 2  | **NEVER** delete or rename existing entities or database columns         |
| 3  | **NEVER** remove audit logging from any operation                        |
| 4  | **NEVER** bypass the posting workflow                                    |
| 5  | **NEVER** add business logic to the UI layer                            |
| 6  | **NEVER** add database access to the Application layer                  |
| 7  | **NEVER** create circular project dependencies                          |
| 8  | **NEVER** use `float` or `double` for financial amounts                  |
| 9  | **NEVER** remove soft-delete protection from financial records           |
| 10 | **NEVER** hard-delete any data in production-targeted code               |
| 11 | **NEVER** modify the composition root without explicit instruction       |
| 12 | **NEVER** skip concurrency control (RowVersion) on entities             |
| 13 | **NEVER** introduce a new external dependency without approval           |
| 14 | **NEVER** disable or suppress any existing test                         |
| 15 | **NEVER** write code that uses `DateTime.Now` directly                  |

---

## 6. Mandatory Pre-Action Checklist

Before the agent creates **any** new code file, it must verify:

### Architecture Check
- [ ] Does this file belong in the correct layer per ARCHITECTURE.md?
- [ ] Does it follow the namespace convention in SOLUTION_STRUCTURE.md?
- [ ] Does it follow the file naming convention?
- [ ] Does it introduce any forbidden cross-layer dependency?

### Financial Check (if applicable)
- [ ] Does this involve a financial entity or transaction?
- [ ] If yes, does it enforce double-entry rules per ACCOUNTING_PRINCIPLES.md?
- [ ] Does it respect the posting workflow per FINANCIAL_ENGINE_RULES.md?
- [ ] Does it respect period lock rules?

### Data Check
- [ ] Does the entity have all mandatory base columns (CreatedAt, RowVersion, etc.)?
- [ ] Is soft delete implemented for financial data?
- [ ] Is the audit log captured for this entity?
- [ ] Are all money fields using `decimal`?

### UI Check (if applicable)
- [ ] Does the window follow UI_GUIDELINES.md layout?
- [ ] Does it bind to DTOs (not domain entities)?
- [ ] Is there NO business logic in the window?
- [ ] Are posted records shown as read-only?

---

## 7. Mandatory Impact Analysis Questions

Before implementing any feature, the agent must answer these questions:

| #  | Question                                                                |
|----|-------------------------------------------------------------------------|
| 1  | **Which layers are affected?** List all projects that will be modified. |
| 2  | **What entities are involved?** List all domain entities affected.      |
| 3  | **Does this affect financial data?** If yes, what integrity rules apply?|
| 4  | **Does this require a new table?** If yes, what columns are mandatory? |
| 5  | **Does this require a new interface?** Where is it defined? Where implemented? |
| 6  | **Does this affect an existing interface?** What existing implementations break? |
| 7  | **What tests are needed?** List the test cases before writing code.    |
| 8  | **What governance documents apply?** List all relevant governance files.|
| 9  | **What is the rollback plan?** How to undo if something goes wrong?    |
| 10 | **Are there any assumptions?** List them explicitly for human review.   |

---

## 8. Code Quality Standards for Agent Output

| Standard                                                                    |
|-----------------------------------------------------------------------------|
| All generated code must compile without warnings.                           |
| All public members must have XML documentation.                             |
| Generated code must follow the established patterns in the codebase.        |
| No `// TODO` without a clear description of what needs to be done.          |
| No placeholder implementations — either implement fully or don't create.   |
| All generated tests must be meaningful (not just `Assert.True(true)`).      |
| Code must be formatted consistently with the rest of the codebase.          |

---

## 9. Agent Communication Protocol

When the agent encounters uncertainty, it must:

1. **Stop** — do not guess or assume
2. **State** — clearly describe what is uncertain
3. **Ask** — formulate a specific question for the human
4. **Wait** — do not proceed until clarification is received

When the agent completes a task, it must report:
1. **What was created** — list of files and their purpose
2. **What was modified** — list of changes to existing files
3. **What governance rules were checked** — which rules were validated
4. **What assumptions were made** — if any
5. **What tests should be run** — to verify the changes

---

## 10. Agent Session Rules

| Rule                                                                        |
|-----------------------------------------------------------------------------|
| At the start of each session, the agent reads all governance documents.     |
| The agent does not carry assumptions between sessions.                       |
| The agent confirms the current phase and scope before starting work.        |
| The agent never claims a feature is "complete" without listing what was done.|
| The agent maintains a clear record of decisions made during the session.    |

---

## Version History

| Version | Date       | Change Description                    |
|---------|------------|---------------------------------------|
| 1.0     | 2026-02-08 | Initial Phase 1 governance release    |
