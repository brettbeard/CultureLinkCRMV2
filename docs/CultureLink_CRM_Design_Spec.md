# Design Specification: CultureLink CRM

**Status**: Ready for implementation
**Source of truth for requirements**: `docs/CultureLink_CRM_SRS.md` (referenced throughout as "SRS")
**Supersedes**: `docs/CultureLink_CRM_Spec.md` (prior spec; retained for historical context only, do not implement from it)

This document is written for a future Claude Code session that has this repository open but has not read the SRS. Every design decision below cites the SRS requirement ID(s) it satisfies. Decisions with no direct SRS citation are architectural necessities and are labeled as such explicitly.

---

## Overview & Goals

CultureLink CRM is a single-tenant, self-hosted internal web application that gives CultureLink staff one system of record for the people, households, and organizations they work with — donors, church/ministry partners, seminar attendees, curriculum customers, and consulting/coaching clients — plus the full history of every donation, seminar, curriculum order, and engagement tied to each of them (Ref: SRS §1). Staff need to filter this data, build named, reusable "communication audiences" out of it with automatic household-level de-duplication, and export any list to Excel for mailing (Ref: FR-9, FR-10, FR-11).

The system is built as a single ASP.NET Core 10 MVC application (Controllers + Razor Views), not a separate API + client pair, following Clean Architecture layering (Core / Infrastructure / Web) per the existing ADR (`docs/decisions/001-initial-architecture.md`) and NFR 3.5's Controllers/Views/Services separation. It authenticates users against its own database (no third-party identity provider) and enforces three roles (Admin, Superuser, User) at the controller/action level (Ref: FR-12, FR-13). The initial persistence provider is SQLite, accessed exclusively through EF Core so the provider can later be swapped for SQL Server/PostgreSQL via configuration alone (Ref: NFR 3.1).

## Non-Goals

The following are explicitly out of scope for this design and must not be built:

- Donor receipt generation (Ref: SRS §5).
- Any public-facing website functionality or modification to culturelinkinc.org — branding alignment only (Ref: FR-14, SRS §5).
- Multi-tenant support of any kind (Ref: SRS §5).
- Third-party identity provider integration — OAuth, Active Directory, or ASP.NET Core Identity as an external/framework-managed identity system (Ref: FR-12, SRS §5). Note: this means auth is hand-rolled (custom `User` table, hand-rolled password hashing, hand-rolled cookie issuance) rather than built on the `Microsoft.AspNetCore.Identity` package.
- Data migration/import tooling (Access-to-CRM conversion) (Ref: SRS §5).
- Account lockout / failed-login throttling (Ref: FR-12).
- Mobile/tablet responsive design — desktop viewports only (Ref: NFR 3.3).
- Network hierarchy rollup reporting or recursive tree UI (Ref: FR-4).
- Type-specific structured fields per Engagement type (Ref: FR-8).
- Segment assignment metadata (Ref: FR-5).
- Persisted/saved Excel export column templates or per-user defaults — column selection is session/request-scoped only (Ref: FR-11).
- A separate HTTP API surface for external consumers — `contracts/openapi.json` remains a stub; this design does not build a Web API project (No direct SRS requirement — architectural necessity, resolved by user decision during spec design: single MVC app, not separate Api + Client projects).
- Arbitrary ad-hoc filter criteria (name, location, date ranges, etc.) as part of a saved Audience definition — Audiences are Segment-only combinations; ad-hoc filtering (FR-9) remains a separate, non-persisted search feature (Ref: FR-9, FR-10, resolved by user decision).

---

## Architecture

### Solution layout under `source/`

```
source/
├── CultureLinkCRM.sln
├── CultureLinkCRM.Core/              Domain entities, enums, interfaces (services, repositories, IEmailSender), no framework deps
├── CultureLinkCRM.Infrastructure/     EF Core DbContext, entity configurations, migrations, repository/service implementations, BCrypt hashing, IEmailSender no-op/SMTP impl
├── CultureLinkCRM.Web/                ASP.NET Core MVC app: Controllers, Views, wwwroot (site CSS/branding assets), Program.cs/composition root
├── CultureLinkCRM.Tests/              xUnit tests — Api-boundary integration tests (WebApplicationFactory-style host over a real SQLite DbContext) as the primary seam, plus focused unit tests for lapsed-donor computation and household de-dup
```

(No direct SRS requirement — architectural necessity, matches AGENTS.md's Core/Infrastructure/Api/Client naming intent, collapsed here into Core/Infrastructure/Web per the single-app decision.)

**Dependency direction** (Ref: AGENTS.md "Key Rules", enforced here as hard rules):
- `Core` has zero project references — no dependency on Infrastructure or Web.
- `Infrastructure` → `Core` only.
- `Web` → `Core` and `Infrastructure` (Infrastructure reference is needed only in the composition root / `Program.cs` for DI registration; controllers must depend on `Core` interfaces, never on `Infrastructure` concrete types).
- Controllers contain no business logic — they call into `Core`-defined service interfaces implemented in `Infrastructure` (Ref: AGENTS.md "Key Rules", NFR 3.5).

### Layer responsibilities

- **Core**: Domain entities (Person, Household, Organization, Network, Segment, Donation, Seminar, SeminarAttendance, CurriculumOrder, Engagement, User, SystemSetting), enums, service interfaces (`IPersonService`, `IHouseholdService`, `IAudienceService`, `IExcelExportService`, `IAuthService`, `IEmailSender`, etc.), and pure domain logic that needs no I/O (e.g., the lapsed-donor-status calculation given a threshold and last-donation date). (Ref: NFR 3.5, AGENTS.md)
- **Infrastructure**: `CultureLinkCrmDbContext` (EF Core), entity type configurations (Fluent API, one `IEntityTypeConfiguration<T>` per entity), EF Core migrations, concrete service implementations (querying, validation orchestration, Household de-dup algorithm, Excel generation via ClosedXML, BCrypt password hashing, `IEmailSender` implementation). (Ref: NFR 3.1, NFR 3.5)
- **Web**: MVC Controllers (one per aggregate: `PersonController`, `HouseholdController`, `OrganizationController`, `NetworkController`, `SegmentController`, `DonationController`, `SeminarController`, `CurriculumOrderController`, `EngagementController`, `AudienceController`, `ExportController`, `AccountController` for login/logout/password-reset, `UserAdminController` for Admin-only user/role management, `SettingsController` for the lapsed-donor threshold), Razor Views, `wwwroot` (branding assets per FR-14), cookie authentication middleware, role-based `[Authorize(Roles = ...)]` attributes, anti-forgery token wiring. (Ref: FR-13, NFR 3.2)

### Cross-cutting concerns

- **Authentication**: Cookie-based auth (ASP.NET Core's cookie authentication *handler*, which is a generic auth transport, not the `Identity` framework/package — this is permitted; only third-party/external identity *providers* and the `Identity` package's own user-management framework are excluded) (Ref: FR-12). Credentials validated against `Core.User` via a hand-rolled `IAuthService`; passwords hashed with BCrypt (Ref: FR-12, NFR 3.2).
- **Authorization**: Three roles stored on `User.Role` (enum: `Admin`, `Superuser`, `User`). Enforced via `[Authorize(Roles = "...")]` on every controller/action touching non-public data; `UserAdminController` and `SettingsController` require `Admin` only (Ref: FR-13, NFR 3.2).
- **Anti-forgery**: `[ValidateAntiForgeryToken]` (or the global auto-validation filter) on every state-changing (`POST`/`PUT`/`DELETE`-equivalent) action (Ref: NFR 3.2).
- **Database provider**: SQLite via `Microsoft.EntityFrameworkCore.Sqlite`, accessed only through `Core`-defined repository/service interfaces so the concrete provider is swappable later purely via `Infrastructure` DI registration + connection string (Ref: NFR 3.1).

---

## Data Models

All entities live in `Core` as POCOs with EF Core Fluent API configuration in `Infrastructure` (no data-annotation attributes on domain entities, to keep `Core` framework-agnostic) (No direct SRS requirement — architectural necessity, matches AGENTS.md's Core purity rule).

Every entity below has `Id` (int, identity PK), `CreatedAt` (DateTime, UTC), and `ModifiedAt` (DateTime, UTC) unless noted — needed to support FR-9's "date added/modified" filter (Ref: FR-9).

### Person (Ref: FR-1)
- `Id`: int
- `FirstName`: string, required
- `LastName`: string, required
- `MiddleName`: string, optional
- `Suffix`: string, optional
- `HouseholdId`: int?, FK to Household, nullable (Ref: FR-1)
- `Addresses`: collection of `PersonAddress`
- `Phones`: collection of `PersonPhone`
- `Emails`: collection of `PersonEmail`
- `OrganizationLinks`: collection of `PersonOrganization` (join with Role/Title)
- `NetworkLinks`: collection of `PersonNetwork` (join)
- `SegmentAssignments`: collection of `SegmentAssignment`
- `CreatedAt`, `ModifiedAt`: DateTime

### PersonAddress / PersonPhone / PersonEmail (Ref: FR-1)
Each is its own table (not a shared polymorphic "ContactInfo" table, to keep type-specific fields clean):
- `PersonAddress`: `Id`, `PersonId` (FK), `Type` (enum: Home, Work, Other), `IsPrimary` (bool), `Street1`, `Street2`, `City`, `StateProvince`, `PostalCode`, `Country`
- `PersonPhone`: `Id`, `PersonId` (FK), `Type` (enum: Home, Work, Mobile, Other), `IsPrimary` (bool), `Number` (string, stored in a normalized international format, e.g. E.164 — validated, not necessarily displayed, in that form) (Ref: FR-1 — international phone support)
- `PersonEmail`: `Id`, `PersonId` (FK), `Type` (enum: Home, Work, Other), `IsPrimary` (bool), `Address` (string, validated via a standard email-format validator)

Validation rule: at most one `IsPrimary = true` row per (Person, Type) — enforced in the service layer (not a DB constraint, since EF Core/SQLite partial unique indexes add complexity disproportionate to a 500-1000 record system) (No direct SRS requirement — architectural necessity, derived from FR-1's "type and a primary flag").

### Household (Ref: FR-2)
- `Id`: int
- `HouseholdName`: string, required, free-text (e.g., "The Ragan Family") — used as the display label everywhere the household appears as a single row (Ref: FR-2, FR-10)
- `Addresses`: collection of `HouseholdAddress` (same shape as `PersonAddress` minus `PersonId`, plus `HouseholdId`)
- `Phones`: collection of `HouseholdPhone`
- `Emails`: collection of `HouseholdEmail`
- `MailPreference`: enum or string field capturing household-level mail preference (Ref: FR-2 — "mail preferences may be set at the Household level"; exact preference values are a UI/lookup concern, not further specified by the SRS)
- `Members`: collection of `Person` (inverse of `Person.HouseholdId`)
- `CreatedAt`, `ModifiedAt`: DateTime

**Effective contact info resolution** (Ref: FR-2, resolved via user decision): For a given Person and contact type (address/phone/email) independently: if `Person.{Type}s` has ≥1 entries, use those; otherwise fall back to `Person.Household.{Type}s`. This is a per-contact-type collection fallback, not a field-level merge. Implemented as `IPersonService.GetEffectiveContactInfo(Person)` returning resolved address/phone/email lists, used by both the Person detail view and the Audience/export row-building logic.

**Deletion rule** (Ref: FR-2): `HouseholdService.Delete(householdId)` throws/returns a validation failure if `Household.Members.Any()`. Enforced in the service layer so it holds regardless of entry point, not just a UI-level check (Ref: FR-2, SRS §6 item 2).

### Organization (Ref: FR-3)
- `Id`: int
- `Name`: string, required
- `OrganizationType`: enum (Church, Ministry, Org, Business, Foundation) — lookup-backed (Ref: FR-3's "church/ministry/org/business/foundation")
- `Addresses`, `Phones`, `Emails`: collections, same shape as Household's
- `PersonLinks`: collection of `PersonOrganization` (inverse)
- `NetworkLinks`: collection of `OrganizationNetwork` (join)
- `SegmentAssignments`: collection of `SegmentAssignment`
- `CreatedAt`, `ModifiedAt`: DateTime

### PersonOrganization (join) (Ref: FR-1, FR-3)
- `Id`, `PersonId` (FK), `OrganizationId` (FK), `RoleTitle` (string, e.g. "Pastor")

### Network (Ref: FR-4)
- `Id`: int
- `Name`: string, required
- `NetworkType`: enum (Agency, DenominationAssociation, Region, MinistryFocus, NetworkingGroup, ConferenceConnection)
- `ParentNetworkId`: int?, nullable self-referencing FK (Ref: FR-4)
- `PersonLinks`: collection of `PersonNetwork` (join)
- `OrganizationLinks`: collection of `OrganizationNetwork` (join)
- `CreatedAt`, `ModifiedAt`: DateTime

No rollup/tree logic is built on `ParentNetworkId` — it is settable via the standard edit form only (Ref: FR-4, SRS §6 item 5). On deletion of a Network that is another Network's parent, deletion is blocked (same "block if referenced" pattern as below) rather than silently nulling children's `ParentNetworkId` (No direct SRS requirement — architectural necessity, kept consistent with the general delete-blocking rule below).

### PersonNetwork / OrganizationNetwork (joins) (Ref: FR-1, FR-3, FR-4)
- `Id`, `PersonId` or `OrganizationId` (FK), `NetworkId` (FK)

### Segment (Ref: FR-5, FR-6)
- `Id`: int
- `Name`: string, required (e.g., "Seminar Alumni–Dallas 2024", "Curriculum Customer")
- `IsComputed`: bool — `true` only for the two system-managed virtual segments, `Donor-Active` and `Donor-Lapsed` (Ref: FR-5, FR-6, resolved via user decision: real Segment rows, flagged, not a code-only special case)
- `CreatedAt`, `ModifiedAt`: DateTime

The two `IsComputed = true` rows (`Donor-Active`, `Donor-Lapsed`) are created via EF Core seed data / migration (fixed, well-known IDs) and can never be created, edited, or deleted through the Segment CRUD screen — the Segment management UI and `SegmentService.Create/Update/Delete` explicitly reject any attempt to mutate a row where `IsComputed = true` (Ref: FR-5, FR-6).

### SegmentAssignment (join) (Ref: FR-5)
- `Id`, `SegmentId` (FK, must reference an `IsComputed = false` Segment — computed segments are never assigned as rows, only evaluated at query time), `PersonId` (FK, nullable), `OrganizationId` (FK, nullable — exactly one of PersonId/OrganizationId is set), `DateAssigned`: DateTime

No additional metadata column, per FR-5.

### Donation (Ref: FR-6)
- `Id`: int
- `PersonId`: int? (FK, nullable)
- `OrganizationId`: int? (FK, nullable) — exactly one of PersonId/OrganizationId is set (a donation is against a Person or an Organization, not both)
- `Amount`: decimal
- `DonationDate`: DateTime
- `FundProjectDesignation`: string
- `CreatedAt`, `ModifiedAt`: DateTime

Currency is assumed single-currency (USD) throughout; the SRS does not raise multi-currency as a concern despite international partnerships, so no `Currency` field is modeled (No direct SRS requirement — architectural necessity; flagged as an open question below).

### SystemSetting (Ref: FR-6)
- `Id`: int
- `Key`: string (e.g., `"LapsedDonorThresholdMonths"`)
- `Value`: string
- A single row with `Key = "LapsedDonorThresholdMonths"`, `Value = "12"` is seeded by default (Ref: FR-6 — configurable, default 12 months). Only `Admin` role can update via `SettingsController` (Ref: FR-13).

**Computed donor status logic** (Ref: FR-6, `Core` pure logic, unit-tested directly per the testing approach below):
```
GetDonorStatus(mostRecentDonationDate, thresholdMonths, asOfDate):
    if mostRecentDonationDate is null: return NoDonationHistory (excluded from both virtual segments)
    if asOfDate - mostRecentDonationDate <= thresholdMonths: return Active
    else: return Lapsed
```
Exposed read-only, evaluated at query time against the `SystemSetting` threshold — never stored as a `SegmentAssignment` row (Ref: FR-6, SRS §6 item 10).

### Seminar (Ref: FR-7)
- `Id`: int
- `City`: string
- `Year`: int
- `ParentSeminarId`: int?, nullable self-referencing FK, for cross-seminar rollup grouping (Ref: FR-7)
- `Attendances`: collection of `SeminarAttendance`

### SeminarAttendance (join) (Ref: FR-7)
- `Id`, `SeminarId` (FK), `PersonId` (FK)

### CurriculumOrder (Ref: FR-7)
- `Id`: int
- `PersonId`: int? (FK, nullable)
- `OrganizationId`: int? (FK, nullable) — exactly one of PersonId/OrganizationId set
- `Quantity`: int
- `OrderDate`: DateTime
- `LinkedOrganizationId`: int? (FK to Organization, nullable — "linked church/ministry if applicable" per FR-7, distinct from the ordering Organization itself when the order is placed by a Person on behalf of a church)
- `CreatedAt`, `ModifiedAt`: DateTime

### Engagement (Ref: FR-8)
- `Id`: int
- `PersonId`: int? (FK, nullable)
- `OrganizationId`: int? (FK, nullable) — exactly one of PersonId/OrganizationId set
- `EngagementType`: string or enum-like lookup value (Consulting, Coaching, STMTrip, InternationalPartnership, etc.), backed by an extensible lookup table (`EngagementType` reference table) rather than a hardcoded enum, since FR-8 calls it "extensible" (Ref: FR-8)
- `StartDate`: DateTime
- `EndDate`: DateTime? (null = ongoing)
- `Notes`: string (free-text)
- `CreatedAt`, `ModifiedAt`: DateTime

Single generic schema for all engagement types, no type-specific structured columns, per FR-8 and SRS §6 item 3.

### Audience (Ref: FR-10)
- `Id`: int
- `Name`: string, required (e.g., "Ministry Reports", "Personal Updates", "Seminar Promotion")
- `SegmentIds`: collection of `AudienceSegment` (join: `AudienceId`, `SegmentId`) — the set of Segments (including `IsComputed` virtual segments) ORed together to define membership (Ref: FR-10, resolved via user decision: live definition, Segments only)
- `CreatedAt`, `ModifiedAt`: DateTime

An Audience is a **live, saved definition**, not a frozen snapshot: `IAudienceService.GetMembers(audienceId)` re-evaluates matching Persons/Organizations against current `SegmentAssignment` (and computed donor-status) data every time it is viewed or exported (Ref: FR-10, resolved via user decision).

### User (Ref: FR-12, FR-13)
- `Id`: int
- `Email`: string, required, unique
- `PasswordHash`: string (BCrypt)
- `Role`: enum (Admin, Superuser, User) (Ref: FR-13)
- `PasswordResetToken`: string?, nullable
- `PasswordResetTokenExpiresAt`: DateTime?, nullable
- `CreatedAt`, `ModifiedAt`: DateTime

A single initial Admin user is created via EF Core seed data at migration/startup time, with email and password sourced from configuration (`appsettings`/environment variables, e.g. `Seed:AdminEmail` / `Seed:AdminPassword`), not hardcoded — only inserted if the `Users` table is empty (Ref: FR-12, resolved via user decision). The Admin is expected to change this password after first login; no forced-change-on-first-login flow is required by the SRS, so none is built (No direct SRS requirement — architectural necessity, kept minimal).

---

## Key Flows

### Flow 1: Create a Person with contact info (Ref: FR-1)
1. Superuser/Admin navigates to `Person/Create`.
2. Form captures name fields, optional Household selection, and repeatable address/phone/email sub-forms (each with Type + IsPrimary).
3. On submit, `PersonController.Create` validates: name required; each email passes format validation; each phone passes international-format validation; at most one `IsPrimary` per (Type) group (Ref: FR-1).
4. On validation failure: redisplay form with field-level errors, nothing saved (Ref: FR-1 acceptance criteria).
5. On success: `PersonService.Create` persists Person + child contact rows in a single transaction; redirect to `Person/Details/{id}`.

### Flow 2: Delete a Household with members attached (Ref: FR-2)
1. Superuser/Admin navigates to `Household/Delete/{id}` (confirmation page) or triggers delete from the list/detail view.
2. `HouseholdController.Delete` (POST) calls `HouseholdService.Delete(id)`.
3. `HouseholdService.Delete` checks `Household.Members.Any()`. If true, returns a validation failure with a message identifying the blocking members; controller re-renders the confirmation page with that error, no deletion occurs (Ref: FR-2 acceptance criteria).
4. User must first reassign/unlink each member Person's `HouseholdId` before retrying delete.

### Flow 3: Compute and filter by donor status (Ref: FR-6, FR-9)
1. Any authenticated user opens the Person or Organization filter/search screen.
2. Donor status is offered as a filter option alongside the two virtual Segments (`Donor-Active`, `Donor-Lapsed`), sourced from the `Segment` table where `IsComputed = true` (Ref: FR-5, FR-6, FR-9).
3. On filter execution, `DonorStatusService` (Core logic + Infrastructure query) computes, per Person/Organization, the most recent `Donation.DonationDate` and evaluates it against the current `SystemSetting["LapsedDonorThresholdMonths"]` value to classify Active/Lapsed/NoHistory, then applies that as a query predicate.
4. Results render in the standard sortable/paginated grid (Ref: FR-9).

### Flow 4: Build and view an Audience (Ref: FR-10)
1. Superuser/Admin/User navigates to `Audience/Create` (User role has access per FR-13, since Audience Builder use is explicitly granted to User).
2. Form: Audience `Name` + multi-select list of Segments (real + virtual, `IsComputed` included) to OR together.
3. On save, `AudienceService.Create` persists the `Audience` + `AudienceSegment` join rows only (no member snapshot).
4. Viewing `Audience/Details/{id}` calls `AudienceService.GetMembers(id)`:
   a. Query all Persons/Organizations with a `SegmentAssignment` to any of the Audience's Segments, or matching computed donor-status Segments via the Flow 3 logic — union (OR) across all selected Segments.
   b. For matched Persons: group by `HouseholdId`. Persons with a non-null `HouseholdId` collapse to one row per Household (Ref: FR-10). Persons with no Household render as individual rows.
   c. Each Household row displays `HouseholdName` and the Household's effective address/contact info (Ref: FR-2, FR-10); the specific member(s) that matched are not listed on that row (Ref: FR-10).
   d. Matched Organizations render as their own rows (Organizations have no Household concept) — see Open Question 1 on how Person and Organization rows combine into a single result set.
5. This de-dup routine lives in exactly one place in `Infrastructure` (e.g., `IAudienceService.GetMembers`) and is reused, not reimplemented, by the Excel export path in Flow 5 (Ref: FR-10, FR-11 — de-dup parity requirement).

### Flow 5: Excel export of a filtered list or Audience (Ref: FR-11)
1. From a Person list, Organization list, or Audience detail view, user clicks "Export to Excel."
2. `ExportController` renders a column-selection screen (checkboxes for available columns for that entity type), scoped to the current request/session only — no persistence of the selection beyond it (Ref: FR-11).
3. On submit, `ExportController` calls the same underlying query/de-dup logic used to render the on-screen list/Audience (Flow 4 step 4's routine, or the equivalent Person/Organization filter query), then `IExcelExportService.Generate(rows, selectedColumns)` builds an `.xlsx` file in-memory (via ClosedXML) and streams it back as a file download (Ref: FR-11).
4. The exported rows are guaranteed to match what was seen on screen because both paths call the identical service method (Ref: FR-11 acceptance criteria).

### Flow 6: Login, logout, and password reset (Ref: FR-12)
1. **Login**: `AccountController.Login` (GET/POST) — POST validates email/password against `User.PasswordHash` via BCrypt verify; on success, issues a cookie-auth ticket containing the user's `Id` and `Role`; redirects to a role-appropriate dashboard (Ref: FR-12 acceptance criteria).
2. **Logout**: `AccountController.Logout` clears the auth cookie, redirects to login.
3. **Password reset request**: `AccountController.ForgotPassword` (POST) — generates a random token + expiry, stores on `User.PasswordResetToken`/`PasswordResetTokenExpiresAt`, calls `IEmailSender.SendPasswordResetEmailAsync(user.Email, resetLink)` (Ref: FR-12). The concrete `IEmailSender` Infrastructure implementation (transactional provider vs. CultureLink's own SMTP relay) is an open decision — see Open Questions.
4. **Password reset completion**: `AccountController.ResetPassword` (GET renders form given a valid, non-expired token; POST) validates token + expiry, updates `PasswordHash`, clears the token fields.

### Flow 7: Role-gated access denial (Ref: FR-13)
1. A `User`-role account attempts to navigate directly to `/UserAdmin` or `/Settings` by URL.
2. `[Authorize(Roles = "Admin")]` on `UserAdminController`/`SettingsController` rejects the request before any action code runs; ASP.NET Core's authorization middleware returns a 403 (or redirects to an "access denied" page) without querying or exposing any user/settings data (Ref: FR-13 acceptance criteria).

---

## Error Handling & Edge Cases

- **Invalid email format** (Ref: FR-1 acceptance criteria): rejected at the service layer (not just client-side), returns a field-level validation error, record not saved.
- **Invalid/non-international phone format** (Ref: FR-1): validated server-side using a library capable of international parsing (e.g., `libphonenumber-csharp`) rather than a US-centric regex (No direct SRS requirement — architectural necessity, driven by FR-1's explicit international requirement).
- **Household delete with members attached** (Ref: FR-2): blocked with a specific, actionable validation message (not a generic "cannot delete" error) naming the count/members blocking it.
- **Network delete when referenced as a ParentNetworkId** (Ref: FR-4): blocked, consistent with the general "block delete if referenced" rule (No direct SRS requirement — architectural necessity, extending FR-2's pattern for consistency).
- **Delete of Person/Organization/Segment with attached history** (donations, engagements, seminar attendance, curriculum orders, segment assignments): blocked with a validation error identifying what's still attached, same pattern as Household (Ref: FR-1, FR-3, FR-5, FR-6, FR-7, FR-8, resolved via user decision).
- **Attempted mutation of a computed Segment** (`Donor-Active`/`Donor-Lapsed`): `SegmentService.Update/Delete` rejects any request where `Segment.IsComputed == true`, regardless of role (Ref: FR-5, FR-6).
- **Donation/CurriculumOrder/Engagement with neither PersonId nor OrganizationId, or both set**: rejected at the service layer — exactly one must be set (No direct SRS requirement — architectural necessity, derived from FR-6/FR-7/FR-8's "against a Person or Organization" phrasing).
- **User (read-only role) attempts create/edit/delete on any core record**: `[Authorize(Roles = "Admin,Superuser")]` on those actions returns 403 before controller logic executes (Ref: FR-13 acceptance criteria).
- **Direct URL access to Admin-only screens by non-Admin**: see Flow 7 — no data exposure, clean denial (Ref: FR-13 acceptance criteria).
- **Password reset token reuse/expiry**: expired or already-consumed tokens are rejected with a generic "link invalid or expired" message; token is single-use (cleared on successful reset) (No direct SRS requirement — architectural necessity, standard reset-flow safety).
- **Excel export with zero selected columns or zero matching rows**: export still produces a valid (header-only, or empty) `.xlsx` rather than erroring, so the user gets a file rather than a dead end (No direct SRS requirement — architectural necessity).
- **Audience with zero selected Segments**: cannot be saved — validation error requiring at least one Segment (No direct SRS requirement — architectural necessity, an Audience with no criteria is meaningless).
- **Lapsed threshold boundary condition** (exactly at the threshold): treated as still Active (`<=` in the `GetDonorStatus` logic above), tested explicitly as a unit test edge case per the testing approach (Ref: FR-6).

---

## Open Questions / Risks

1. **Mixed Person + Organization rows in one Audience/export result set** (Ref: FR-10, FR-11): the SRS's Household de-dup and `HouseholdName` display rule is Person-specific; Organizations have no Household. If a Segment is assigned to both Persons and Organizations, does a single Audience view/export interleave both entity types in one grid (with different available columns), or should Audiences implicitly be scoped to one entity type at a time? This design assumes both can appear in one result set as separate row "kinds," but the exact column/grid presentation needs a decision before building the Audience/export views.
2. **Multi-currency donations**: this design assumes single-currency (USD) `Donation.Amount` with no `Currency` field, despite CultureLink's international partnerships (Ref: FR-6, FR-8). Confirm this assumption before building donation entry forms if international currency donations are ever recorded directly (as opposed to being converted to USD before entry).
3. **Concrete `IEmailSender` implementation**: deferred per FR-12 pending CultureLink's choice of mail system (transactional provider vs. own SMTP relay). The interface and reset flow (Flow 6) are buildable now; only the concrete Infrastructure class is blocked (Ref: FR-12).
4. **Household `MailPreference` value set**: FR-2 requires the field to exist but does not enumerate its allowed values (e.g., "Mail to Household," "Mail to Individual," "Do Not Mail"). A concrete lookup list should be confirmed with CultureLink before finalizing the enum (Ref: FR-2).
5. **Pagination page size default and sort defaults** for FR-9's grids are not specified by the SRS; this design assumes a reasonable default (e.g., 25/50 rows per page) to be confirmed or made configurable (Ref: FR-9).

---

## Implementation Notes

- **Follow AGENTS.md's coding conventions**: async/await throughout, constructor DI only, no static state, file-scoped namespaces, C# 14+ features welcome (Ref: AGENTS.md).
- **Follow AGENTS.md's key rules**: Core must never reference Infrastructure; controllers must never contain business logic; every service has a Core-defined interface (Ref: AGENTS.md).
- **EF Core migrations**: create via `dotnet ef migrations add <Name>` from the Infrastructure project, tracked in source control under `Infrastructure/Migrations/` (the existing empty `database/migrations/` directory is superseded by EF Core's own migrations folder for this design — do not hand-write parallel SQL migration files) (Ref: NFR 3.1).
- **Password hashing**: use `BCrypt.Net-Next` (or equivalent maintained BCrypt package); never roll a custom hashing scheme (Ref: FR-12, NFR 3.2).
- **International phone validation**: use `libphonenumber-csharp` (Google's libphonenumber port) rather than a hand-written regex, given FR-1's explicit international requirement.
- **Excel generation**: use ClosedXML (MIT-licensed, no external Excel dependency) for `.xlsx` generation in `IExcelExportService` (No direct SRS requirement — architectural necessity, standard choice for this ecosystem).
- **Testing approach** (carried forward from the prior spec's testing decisions, still applicable): primary seam is integration tests driving `Web` controllers against a real EF Core `DbContext` backed by SQLite; supplement with focused unit tests for the lapsed-donor threshold calculation (Ref: FR-6) and the Household-level Audience/export de-duplication routine (Ref: FR-10, FR-11), since both have precise edge-case behavior worth testing directly.
- **Branding**: source actual logo, color palette, and typography from culturelinkinc.org's style guide or site CSS before building `wwwroot`/shared layout — do not invent placeholder brand colors and ship them (Ref: FR-14).
- **Do not build**: ASP.NET Core Identity package usage, OAuth/AD integration, account lockout logic, mobile-responsive CSS, Network rollup/tree UI, per-Engagement-type structured fields, Segment assignment metadata columns, or saved/persisted export column templates — all explicitly out of scope (see Non-Goals).
