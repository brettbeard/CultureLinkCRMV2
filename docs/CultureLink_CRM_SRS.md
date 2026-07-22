# Software Requirements Specification: CultureLink CRM

## 1. Purpose & Scope

CultureLink CRM is a single-tenant, self-hosted web application for managing contacts (individuals, households, and organizations), their affiliations and network memberships, and related engagement history (donations, seminars, curriculum orders, consulting/coaching), with role-based user access and the ability to build filtered communication audiences and export them to Excel.

---

## 2. Functional Requirements

### FR-1: Person Management
- Create, view, edit, and delete Person records.
- Each Person supports: first/last name (and optionally middle name/suffix), one or more addresses, one or more phone numbers, one or more emails.
- Each address/phone/email entry has a type (e.g., Home, Work, Mobile) and a primary flag.
- Email fields must pass format validation. Phone fields must accept and validate **international formats** (not US-only), since CultureLink maintains international ministry partnerships (see FR-8).
- A Person may optionally belong to a Household.
- A Person may be linked to zero or more Organizations concurrently, each with a Role/Title (e.g., "Pastor at First Baptist").
- A Person may be linked to zero or more Networks.

### FR-2: Household Management
- Create, view, edit, and delete Household records.
- Each Household has a `HouseholdName` field (free-text, manually set, e.g., "The Ragan Family") used as its display label in lists and exports.
- Address, phone, email, and mail preferences may be set at the Household level.
- Any Person-level contact info overrides the Household-level default for that individual.
- Deleting a Household is **blocked while it has member Persons attached**. The user must reassign or unlink all members before the Household record can be deleted; the system returns a clear validation error if deletion is attempted with members still present.

### FR-3: Organization Management
- Create, view, edit, and delete Organization (church/ministry/org/business/foundation) records.
- Each Organization supports one or more addresses, phone numbers, and emails, structured the same way as Person contact info.
- Each Organization can display its linked Persons (staff/members/affiliates) and linked Networks.

### FR-4: Network Management
- Create, view, edit, and delete Network records (Agency, Denomination/Association, Region, Ministry Focus, Networking Group, Conference Connection).
- A Network may optionally have a parent Network (self-referencing hierarchy: a nullable `ParentNetworkId` FK), supporting nested groupings.
- *Note*: hierarchy usage is not yet defined by the business. The field exists and is settable via the standard edit form, but no rollup reporting, recursive tree UI, or other logic built on top of it is in scope until a concrete use case is defined.

### FR-5: Segment / Tag Management
- Create and manage a reusable list of Segments/Tags (e.g., Seminar Alumni–[City/Year], Curriculum Customer, personal mailing lists).
- A Person or Organization may be assigned to multiple Segments simultaneously.
- Segment assignment is a plain join (Segment, Person/Organization, date assigned) with no additional metadata column — no current Segment type requires per-assignment metadata.
- Donor–Active and Donor–Lapsed are **not** manually assignable Segments; they are computed/virtual Segments derived from donor status (see FR-6) and appear as filter options in Segment-based screens and the Audience Builder (FR-10), but are never editable via the Segment management CRUD screen.

### FR-6: Donation Tracking
- Record donations against a Person or Organization: amount, date, and fund/project designation.
- System computes Active vs. Lapsed donor status based on a **configurable** lapsed-time threshold (system setting, not hardcoded). Default threshold: **12 months** since last donation.
- Computed donor status is exposed as a read-only virtual Segment (see FR-5) usable in filters and the Audience Builder — it is never a manually-assigned tag, keeping donor status single-source-of-truth.
- System computes donation frequency and full donation history per donor.
- Donor receipt generation is out of scope (see Section 5).

### FR-7: Seminar & Curriculum Tracking
- Record Seminars (city, year), with optional parent grouping for cross-seminar rollups.
- Track Person attendance at Seminars (many-to-many).
- Record Curriculum Orders (Person or Organization, quantity, date, linked church/ministry if applicable) with full order history per contact.

### FR-8: Engagement Tracking (Consulting, Coaching, Partnerships)
- Record structured Engagement entries against a Person or Organization: engagement type (Consulting, Coaching, STM Trip, International Partnership, etc., from an extensible lookup list), start date, optional end date (ongoing if blank), and free-text details/notes.
- All Engagement types share a single generic schema (type, start date, end date, free-text notes). No type-specific structured fields (e.g., a dedicated team-size field for STM Trips) are in scope; any such detail is captured in the free-text notes.

### FR-9: Filtering & Search
- Filter Person and Organization records independently by relevant fields: name, location, Segment membership, Network affiliation, donor status, date added/modified.
- Support sortable, paginated results grids.

### FR-10: Audience Builder
- Combine multiple Segments/filters using OR logic to build a named communication audience (e.g., Ministry Reports, Personal Updates, Seminar Promotion), reflecting the predefined audience compositions discussed with the business.
- Resulting audience list must be de-duplicated at the Household level: if multiple members of the same Household match, the Household appears once in the resulting audience.
- When a Household matches, it appears as a **single household-level row**, displayed using the Household's `HouseholdName` and address/contact fields. Individual member names that triggered the match are not listed in the row.

### FR-11: Excel Export
- Export any filtered Person list, Organization list, or built Audience to a downloadable `.xlsx` file.
- Support configurable/selectable columns in the export. Column selection is **session-only** — the user picks columns at export time; no per-user saved defaults or named, reusable export templates are in scope.
- Export must apply the same Household-level de-duplication rule as FR-10.

### FR-12: Authentication
- Application performs its own authentication against the application database (no third-party identity provider).
- Passwords stored using a secure hashing algorithm (e.g., BCrypt or PBKDF2) — plaintext or reversible storage is disallowed.
- Support login, logout, and password reset (reset requires email delivery capability). The password-reset flow is built against an `IEmailSender` interface defined in Core; the concrete email-delivery implementation (transactional provider vs. the organization's own SMTP relay) is deferred to Infrastructure pending CultureLink's decision on which mail system to use.
- Account lockout after repeated failed login attempts is **out of scope for v1**. Security baseline relies on password hashing, HTTPS, anti-forgery protection (see 3.2), and role-based access control.

### FR-13: Authorization / Roles
- Three roles: Admin, Superuser, User.
  - Admin: full access, including User/Role management and system settings.
  - Superuser: full CRUD on all contact/domain data, no User/Role management access.
  - User: read access to all contact/domain data (Person, Household, Organization, Network, Segment, Donation, Seminar, Curriculum, Engagement records), plus the ability to use the Audience Builder and Excel Export. No create, edit, or delete access to any core records.
- Role checks enforced at the controller/action level.

### FR-14: Branding & Visual Consistency
- Application visual design (logo, color palette, typography, general tone) should align with the organization's public website (culturelinkinc.org), so the internal tool feels like a companion system rather than an unrelated product.
- Actual brand colors and font assets must be sourced directly from the organization (style guide or site CSS/theme files), not guessed.

---

## 3. Non-Functional Requirements

### 3.1 Data Integrity
- Database engine must be provider-agnostic at the data-access layer (initial provider: SQLite), enabling a future switch to SQL Server/PostgreSQL via configuration rather than code changes.
- Schema changes must be tracked via versioned migrations.
- Referential integrity must be enforced across Person, Household, Organization, Network, and their join/relationship tables.

### 3.2 Security
- All authentication credentials stored using industry-standard password hashing.
- Anti-forgery protection required on all state-changing form submissions.
- Role-based access control enforced on all controllers/actions handling non-public data.

### 3.3 Usability / Compatibility
- Single-tenant deployment (no multi-organization/tenant isolation required).
- Application is an internal admin tool; supports desktop browser use only. Mobile/tablet responsive design is **not** in scope for v1 — grid-heavy and form-heavy workflows (results grids, multi-field forms, Excel export) are designed and tested for desktop viewports.

### 3.4 Scalability
- Designed for a small dataset (approx. 500–1,000 contact records); no high-concurrency or high-volume performance requirements identified.

### 3.5 Maintainability
- Codebase should follow MVC separation of concerns (Controllers/Views/Services) to keep data access, business logic, and presentation decoupled, supporting the database-agnostic goal above.

---

## 4. Acceptance Criteria

### FR-1: Person Management
**Given** a logged-in user with Superuser or Admin role
**When** they submit a new Person record with a valid name and at least one contact method (address, phone, or email)
**Then** the record is saved and appears in the Person list with all entered contact details correctly associated to it.

**Given** a Person record is submitted with an invalid email format
**When** the form is submitted
**Then** the system rejects the submission and displays a validation error without saving the record.

### FR-10 / FR-11: Audience Builder & Excel Export
**Given** an Audience is built by combining the "Alumni" and "Church Partners" segments
**When** the same Household has two members matching both segments
**Then** the resulting audience list and its Excel export contain that Household only once.

**Given** a user selects an Audience and requests an Excel export
**When** the export is generated
**Then** a downloadable `.xlsx` file is produced containing the selected columns for every de-duplicated contact in the audience.

### FR-12 / FR-13: Authentication & Authorization
**Given** a user without the Admin role
**When** they attempt to access the User Management screen directly via URL
**Then** the system denies access and redirects or returns an authorization error, without exposing user data.

**Given** valid login credentials
**When** a user submits the login form
**Then** the system authenticates the user, establishes a session, and routes them to the dashboard appropriate to their role.

---

## 5. Out of Scope

- **Donor receipt generation** — explicitly handled by the organization's accountant outside this system.
- **Public-facing website functionality** — this application is an internal admin/CRM tool; it does not replace or modify culturelinkinc.org, only aligns with its visual branding (FR-14).
- **Multi-tenant support** — confirmed single-tenant only.
- **Third-party identity provider integration** (e.g., OAuth, Active Directory, ASP.NET Identity) — authentication is self-implemented against the application database by explicit requirement.
- **Data migration/import tooling** — treated as a related but separate workstream (Access-to-CRM conversion and seeding), not part of this SRS's core application scope.

---

## 6. Resolved Decisions Log

All clarifications originally raised in this document have been resolved (2026-07-18) and folded into the relevant sections above. This log preserves the question and rationale for traceability.

1. **User role permissions** (FR-13) — *Resolved*: User role gets read access to all domain data plus Audience Builder/Excel Export use; no create/edit/delete. Rationale: the SRS's own audience names ("Ministry Reports," "Personal Updates") imply a staff role whose job is pulling mailing lists, not maintaining records — pure read-only would block that legitimate workflow.
2. **Household deletion behavior** (FR-2) — *Resolved*: Deletion is blocked while member Persons exist; user must reassign/unlink first. Rationale: low-volume admin tool, so the extra friction is cheap and avoids silent, surprising data changes.
3. **Engagement type field structure** (FR-8) — *Resolved*: One generic schema for all Engagement types; no type-specific structured fields. Rationale: no concrete need for structured per-type fields was identified; adding them would be speculative complexity.
4. **Household export/display format** (FR-10/FR-11) — *Resolved*: Single household-level row per match, using a new `HouseholdName` field as the display label. Rationale: keeps de-dup simple and avoids inventing surname-matching logic to auto-derive a label.
5. **Network hierarchy usage** (FR-4) — *Resolved*: The `ParentNetworkId` self-FK field exists and is editable, but no rollup reporting or tree UI is built until a concrete business use case is defined. Rationale: satisfies the stated requirement that the field exist, without speculative logic on top.
6. **Lapsed donor threshold default** (FR-6) — *Resolved*: Default is 12 months, admin-configurable. Rationale: aligns with a standard annual giving cycle and is easy to widen later if needed.
7. **Phone number format validation** (FR-1) — *Resolved*: International formats are accepted, not US-only. Rationale: FR-8's "International Partnership" engagement type confirms CultureLink has non-US contacts.
8. **Account lockout policy** (FR-12) — *Resolved*: Not built for v1. Security baseline is password hashing, HTTPS, anti-forgery protection, and RBAC.
9. **Mobile/responsive support** (NFR 3.3) — *Resolved*: Desktop-only; no responsive design effort for v1. Rationale: this is a grid-heavy, form-heavy internal admin tool with no stated field/mobile use case.
10. **Donor Active/Lapsed as a Segment** (FR-5 vs. FR-6 conflict) — *Resolved*: Computed donor status is exposed as a read-only virtual Segment, never a manually-assigned tag. Rationale: avoids a manually-tagged status silently drifting from the real computed value.
11. **Password reset email delivery mechanism** (FR-12) — *Resolved*: Deferred behind an `IEmailSender` interface in Core; concrete Infrastructure implementation (transactional provider vs. organization SMTP relay) is a follow-up decision pending input from CultureLink on their mail system.
12. **Excel export column selection persistence** (FR-11) — *Resolved*: Session-only column selection; no saved per-user defaults or named templates. Rationale: the SRS never asked for reuse/persistence, and re-selecting columns is low-cost for an infrequent admin task.
13. **Segment assignment metadata** (FR-5) — *Resolved*: Dropped. The original justification (a Donor segment referencing fund/project designation) no longer applies once Donor status became a computed virtual Segment (#10); no other Segment type has a concrete metadata need.
