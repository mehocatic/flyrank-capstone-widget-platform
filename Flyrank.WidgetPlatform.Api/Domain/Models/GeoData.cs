namespace Flyrank.WidgetPlatform.Api.Domain.Models;

public sealed record GeoData(
    string? Country,
    string? Region,
    string? City,
    double? Latitude,
    double? Longitude);
