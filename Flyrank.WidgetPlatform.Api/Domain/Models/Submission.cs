using System.Text.Json;

namespace Flyrank.WidgetPlatform.Api.Domain.Models;

public sealed record Submission(
    Guid Id,
    Guid WidgetId,
    JsonElement VisitorData,
    string? IpAddress,
    GeoData? GeoData,
    bool IsSpam,
    DateTimeOffset CreatedAt);
