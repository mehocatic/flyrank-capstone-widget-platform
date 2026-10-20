using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Flyrank.WidgetPlatform.Api.Data;
using Flyrank.WidgetPlatform.Api.Middleware;
using Flyrank.WidgetPlatform.Api.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var allowedOrigins = builder.Configuration
    .GetSection("PublicSubmission:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("widget-embed", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods("GET", "POST", "OPTIONS")
                .WithHeaders("Content-Type");
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests",
            Detail = "Please wait before submitting again."
        }, cancellationToken);
    };

    options.AddPolicy("public-submissions", context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 10),
                Window = TimeSpan.FromSeconds(
                    builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60)),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddHttpClient("geo-lookup", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddSingleton<IWidgetPlatformStore, InMemoryWidgetPlatformStore>();
builder.Services.AddScoped<IWidgetService, WidgetService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IGeoEnrichmentService, GeoEnrichmentService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
