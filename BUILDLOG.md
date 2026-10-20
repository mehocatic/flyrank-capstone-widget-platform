# Build Log

This phase-based log summarizes the implementation visible in the current repository. The repository does not contain dated implementation notes, so the phase ordering describes the delivered components and is not a claim about historical commit dates.

## Phase 1: API foundation

- Created the `Flyrank.WidgetPlatform.Api` ASP.NET Core Web API project targeting .NET 11.
- Added controller routing, JSON enum handling, OpenAPI support for Development, problem details, and exception handling.
- Configured the local HTTP launch profile on `http://localhost:5273`.

## Phase 2: Widget management and embeds

- Added tenant-scoped widget listing, lookup, creation, update, and deletion.
- Added public widget configuration, embed snippet, and JavaScript loader endpoints.
- Defined request validation and widget response contracts.

## Phase 3: Public submissions and dashboard

- Added public lead submission with a 65,536-byte request limit.
- Added honeypot classification and a per-remote-IP fixed-window submission limiter.
- Added a tenant-scoped dashboard response with total, spam, valid, and recent submission data.

## Phase 4: Geo enrichment and local integration

- Added optional trusted-proxy geo headers and external IP lookup with an HTTP timeout and graceful fallback.
- Added a static test site served on port 8000 to exercise the API from a second origin.
- Configured the local Development CORS origins.

## Phase 5: Submission documentation

- Added root-level `README.md`, `EVIDENCE.md`, `BUILDLOG.md`, `capstone.yaml`, and `.env.example`.
- Documented the architecture, tenant scoping, local run steps, API routes, and manual evaluation procedures.
- Kept evidence claims aligned with the implementation and marked checks as procedures rather than completed test results.

## AI assistance record

AI assistance was used in this documentation phase to inspect the project structure and implementation, draft the submission documents, and cross-check documented routes and configuration against source files. No git commit was created as part of this work. No automated test project was found during repository inspection.
