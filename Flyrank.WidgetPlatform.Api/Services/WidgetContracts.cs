using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Flyrank.WidgetPlatform.Api.Domain.Models;

namespace Flyrank.WidgetPlatform.Api.Services;

/// <summary>Request data for creating a widget.</summary>
public sealed record CreateWidgetRequest
{
    [NotEmptyGuid]
    public required Guid TenantId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, StringLength(50, MinimumLength = 1)]
    public required string Type { get; init; }

    [Required, StringLength(200, MinimumLength = 1)]
    public required string Title { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string ButtonText { get; init; }
}

/// <summary>Request data for replacing a widget's editable properties.</summary>
public sealed record UpdateWidgetRequest
{
    [NotEmptyGuid]
    public required Guid TenantId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Required, StringLength(50, MinimumLength = 1)]
    public required string Type { get; init; }

    [Required, StringLength(200, MinimumLength = 1)]
    public required string Title { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string ButtonText { get; init; }
}

/// <summary>Widget data returned by the API.</summary>
public sealed record WidgetResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Type,
    string Title,
    string ButtonText,
    DateTimeOffset CreatedAt);

/// <summary>Public configuration consumed by an embedded widget.</summary>
public sealed record WidgetConfigurationResponse(
    Guid Id,
    string Type,
    string Title,
    string ButtonText);

/// <summary>Embed script information for a widget.</summary>
public sealed record WidgetEmbedResponse(string Script);

/// <summary>Request body for a public widget submission.</summary>
public sealed record CreateSubmissionRequest
{
    [NotEmptyGuid]
    public required Guid WidgetId { get; init; }

    [JsonObject]
    public required JsonElement VisitorData { get; init; }

    [StringLength(200)]
    public string? Website { get; init; }
}

/// <summary>Receipt returned after a submission has been accepted.</summary>
public sealed record SubmissionReceiptResponse(Guid SubmissionId, bool Accepted);

/// <summary>Submission data returned to a tenant dashboard.</summary>
public sealed record SubmissionResponse(
    Guid Id,
    Guid WidgetId,
    System.Text.Json.JsonElement VisitorData,
    GeoData? GeoData,
    bool IsSpam,
    DateTimeOffset CreatedAt);

/// <summary>Submission counts for a widget dashboard.</summary>
public sealed record WidgetDashboardResponse(
    Guid WidgetId,
    int TotalSubmissions,
    int SpamSubmissions,
    int ValidSubmissions,
    IReadOnlyList<SubmissionResponse> RecentSubmissions);

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
internal sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class JsonObjectAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is JsonElement element && element.ValueKind == JsonValueKind.Object;
}
