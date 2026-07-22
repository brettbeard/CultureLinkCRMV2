# AGENTS.md

## Project

**CultureLinkCRM**

A CRM web application built on ASP.NET Core 10 targeting cultural and community organizations.

## Status

Scaffold only — no application code exists yet. The `source/` directory is empty. The Visual Studio solution and `.csproj` projects have not been created.

## Intended Tech Stack

- ASP.NET Core 10
- C# 14
- Entity Framework Core
- SQL Server
- Razor Pages (client)
- xUnit (tests)

## Project Structure

```
CultureLinkCRM/
├── .github/workflows/   CI/CD pipeline definitions (build, deploy)
├── docs/                Human-facing documentation (architecture, onboarding, deployment, API, ADRs)
├── ai/                  AI agent context and conventions
├── source/              Application source code (empty — solution not yet scaffolded)
├── contracts/           API contract artifacts (OpenAPI spec)
├── database/            Schema, seed data, and migration scripts
├── build/               Docker build assets
└── tools/               Developer/operator scripts
```

## Next Steps for Agents

1. Scaffold the Visual Studio solution and all project subdirectories into `source/`
2. Implement Core domain models
3. Implement Infrastructure/EF Core
4. Implement Api controllers
5. Implement Client pages

## Coding Conventions

- async/await throughout
- Constructor DI only
- No static state
- C# 14+ features welcome
- File-scoped namespaces

## Key Rules

- Core must never reference Infrastructure
- Controllers must never contain business logic
- All services must have an interface defined in Core
