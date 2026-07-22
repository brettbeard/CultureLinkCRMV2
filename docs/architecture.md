# Architecture

This document describes the intended layered architecture of CultureLinkCRM: how the Api, Core, Infrastructure, and Client projects relate to one another, the direction dependencies are allowed to flow, and the key technology choices underpinning the system.

## Layered Design (Api / Core / Infrastructure / Client)

TODO: fill this in — describe the responsibilities of each layer (Core as the domain/business layer, Infrastructure as persistence/external integrations, Api as the HTTP boundary, Client as the Razor Pages presentation layer) and how they map to projects under `source/`.

## Dependency Flow

TODO: fill this in — document the allowed dependency direction (Api → Core, Infrastructure → Core, Client → Api/Core) and explicitly call out disallowed references (e.g. Core must never reference Infrastructure).

## Key Technology Choices

TODO: fill this in — record why ASP.NET Core 10, EF Core, SQL Server, and Razor Pages were chosen, and any alternatives considered.
