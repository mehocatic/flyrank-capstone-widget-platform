# Flyrank Widget Platform

Flyrank Widget Platform is a .NET 11 ASP.NET Core API for creating embeddable lead-capture widgets, accepting public submissions from other origins, and displaying tenant-scoped submission summaries.

## Features

- Tenant-scoped widget listing, retrieval, update, and deletion.
- Public widget configuration, loader script, and embed snippet endpoints.
- Cross-origin lead submission with CORS support for configured origins.
- Request validation, a submission size limit, and per-IP fixed-window rate limiting.
- Honeypot-based spam classification. Submissions that fill the hidden `website` field are recorded as spam instead of being rejected.
- Optional geo enrichment using trusted proxy headers or an HTTPS IP lookup service.
- A dashboard endpoint with total, spam, and valid submission counts.

## Architecture

```text
External site or embedded widget
            |
            | HTTPS/HTTP, CORS for configured public endpoints
            v
  ASP.NET Core controllers
      |       |         |
      |       |         +--> GeoEnrichmentService --> Trusted proxy headers
      |       |                                  or optional IP lookup
      |       v
      |  SubmissionService
      |       |
      +-------+----------------------+
                                      v
                          IWidgetPlatformStore
                          InMemoryWidgetPlatformStore
                           widgets + submissions
                                      |
                                      v
                         Tenant-scoped dashboard
```

The API project is in `Flyrank.WidgetPlatform.Api`. Controllers handle HTTP input and output, service classes implement widget and submission behavior, and `IWidgetPlatformStore` abstracts the current in-memory store. The in-memory implementation is suitable for local evaluation, but its data is lost when the process stops and it is not a production persistence layer.

## Tenant isolation

Each widget is created with a non-empty `tenantId`. The widget list, widget retrieval, update, and deletion operations check that tenant ID against the widget's owner. The dashboard also requires a `tenantId` and only returns results if the requested widget belongs to that tenant. A widget belonging to a different tenant is treated as not found.

Public configuration and submission endpoints are designed for visitors and therefore identify a widget by its public widget ID rather than requiring a tenant credential. Dashboard and widget-management routes in this project do not implement user authentication or authorization. Tenant IDs are therefore scoping values, not proof of identity. A production deployment must add authenticated tenant identity and durable storage before treating this API as a secure multi-tenant service.

## Requirements

- .NET 11 SDK compatible with the project's target framework.
- Python 3 for the static test site, or Node.js and npm as an alternative.

## Run locally

Start the API from the solution root:

```powershell
dotnet run --project .\Flyrank.WidgetPlatform.Api\Flyrank.WidgetPlatform.Api.csproj --launch-profile http
```

The `http` launch profile listens at `http://localhost:5273` and uses the Development environment. The test site is a separate origin, served on port 8000. Start it in a second terminal:

```powershell
python -m http.server 8000 --directory .\test-site
```

Open `http://localhost:8000`. The Development settings allow this origin. The test site sends JSON to the submission endpoint, so a browser makes a CORS `OPTIONS` preflight before the `POST`.

To use another test-site port, add its complete origin, including scheme and port, to `PublicSubmission:AllowedOrigins` in `Flyrank.WidgetPlatform.Api\appsettings.Development.json`, then restart the API. CORS is not configured to allow arbitrary origins.

## Create a widget and submit a lead

Create a widget using a non-empty tenant ID:

```powershell
$tenantId = "11111111-1111-4111-8111-111111111111"
$body = @{
  tenantId = $tenantId
  name = "Capstone demo"
  type = "lead-capture"
  title = "Contact us"
  buttonText = "Send"
} | ConvertTo-Json

$widget = Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5273/api/widgets" `
  -ContentType "application/json" `
  -Body $body

$widget | ConvertTo-Json
```

Copy the returned `id` into the Widget ID field on the test site. Submit a normal lead with the hidden `website` field empty. To verify honeypot handling, fill in that field using the browser developer tools and submit again. A non-empty honeypot is accepted and classified as spam.

## API routes

Unless noted as public, widget management routes require a `tenantId` query parameter or a `tenantId` in the request body. IDs are GUIDs. Validation errors use HTTP 400, and resources not found in the requested tenant scope use HTTP 404.

### Widgets: `/api/widgets`

| Method and path | Description | Success response |
| --- | --- | --- |
| `GET /api/widgets?tenantId={tenantId}` | List widgets for a tenant. | `200 OK` with an array. |
| `GET /api/widgets/{id}?tenantId={tenantId}` | Get one widget owned by a tenant. | `200 OK`; `404 Not Found` if missing or owned by another tenant. |
| `POST /api/widgets` | Create a widget. Body: `tenantId`, `name`, `type`, `title`, and `buttonText`. | `201 Created` with the widget. |
| `PUT /api/widgets/{id}` | Replace editable widget properties. Body: `tenantId`, `name`, `type`, `title`, and `buttonText`. | `200 OK`; `404 Not Found` if missing or owned by another tenant. |
| `DELETE /api/widgets/{id}?tenantId={tenantId}` | Delete a tenant-owned widget. | `204 No Content`; `404 Not Found` if missing or owned by another tenant. |
| `GET /api/widgets/{id}/configuration` | Get public configuration for an embed. | `200 OK`; `404 Not Found` if missing. |
| `GET /api/widgets/{id}/loader.js` | Get the JavaScript loader for an embed. | `200 OK` with JavaScript; `404 Not Found` if missing. |
| `GET /api/widgets/{id}/embed` | Get an HTML script snippet for a widget. | `200 OK`; `404 Not Found` if missing. |

Example create body:

```json
{
  "tenantId": "11111111-1111-4111-8111-111111111111",
  "name": "Capstone demo",
  "type": "lead-capture",
  "title": "Contact us",
  "buttonText": "Send"
}
```

### Submissions: `/api/submissions`

| Method and path | Description | Responses |
| --- | --- | --- |
| `POST /api/submissions` | Publicly accept a submission for an existing widget. | `202 Accepted`; `400 Bad Request` for invalid input; `404 Not Found` for an unknown widget; `429 Too Many Requests` when the IP limit is exceeded. |

The JSON body contains a non-empty `widgetId`, an object-valued `visitorData`, and an optional `website` honeypot string:

```json
{
  "widgetId": "22222222-2222-4222-8222-222222222222",
  "visitorData": {
    "name": "Alex Example",
    "email": "alex@example.test",
    "message": "Please contact me."
  },
  "website": ""
}
```

The endpoint accepts up to 65,536 request bytes. The default rate limit is 10 requests per IP address per 60-second window, with no queue. These defaults can be changed with `RateLimiting:PermitLimit` and `RateLimiting:WindowSeconds`.

### Dashboard: `/api/dashboard`

| Method and path | Description | Success response |
| --- | --- | --- |
| `GET /api/dashboard/widgets/{widgetId}?tenantId={tenantId}` | Get submission totals and up to 20 recent submissions for a tenant-owned widget. | `200 OK`; `404 Not Found` if the widget is missing or belongs to another tenant. |

The response includes `totalSubmissions`, `spamSubmissions`, `validSubmissions`, and `recentSubmissions`.

## Configuration

Default configuration is in `Flyrank.WidgetPlatform.Api\appsettings.json`; local development origins are in `Flyrank.WidgetPlatform.Api\appsettings.Development.json`. ASP.NET Core configuration environment variables use double underscores to represent nested keys, for example `RateLimiting__PermitLimit`. See [.env.example](./.env.example) for placeholder values. ASP.NET Core does not load `.env` files by default; set those values in the process environment or your IDE's launch configuration.

Geo enrichment is optional. Trusted proxy headers are only used when the immediate remote address is explicitly configured as trusted. External IP lookup is disabled by default and only allows an HTTPS lookup URL outside Development.

## Evaluation notes

The current store is in-memory, so create the widget again after restarting the API. See [EVIDENCE.md](./EVIDENCE.md) for implementation evidence and repeatable manual checks, and [capstone.yaml](./capstone.yaml) for the local run, seed, and evaluation route manifest.
