# Flyrank Test Site

This page simulates an external website running at `http://localhost:8000` and sends lead submissions to `http://localhost:5273/api/submissions`.

## Prerequisites

- .NET SDK installed
- Python installed, or Node.js and npm installed

## Run the API

From the solution root, start the API:

```powershell
dotnet run --project Flyrank.WidgetPlatform.Api --launch-profile http
```

The HTTP launch profile listens at `http://localhost:5273`.

## Run the test site

Open a second terminal at the solution root and start the static site on a different local port:

```powershell
python -m http.server 8000 --directory test-site
```

Alternatively, if Node.js and npm are installed:

```powershell
npx serve test-site --listen 8000
```

Open `http://localhost:8000` in your browser. This is a different origin from the API, which lets you verify the CORS preflight and submission request.

## Submit a test lead

1. Create a widget by sending a `POST` request to `http://localhost:5273/api/widgets`, or use an existing widget.
2. Enter the widget's `id` in the **Widget ID** field.
3. Fill in the visitor details and submit the form.
4. Leave the hidden honeypot field empty for a normal submission. To test spam detection, fill the `website` field using your browser's developer tools before submitting. A non-empty honeypot value is accepted but flagged as spam.

The development CORS configuration allows the test site's origin, `http://localhost:8000`. Because the request uses the JSON `Content-Type`, the browser sends an `OPTIONS` preflight request before the `POST`.

## Use a different test-site port

If you run the test site on another port, such as `8080`, add its full origin, for example `http://localhost:8080`, to `PublicSubmission:AllowedOrigins` in `Flyrank.WidgetPlatform.Api/appsettings.Development.json`, then restart the API.
