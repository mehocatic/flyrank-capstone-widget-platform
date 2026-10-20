namespace Flyrank.WidgetPlatform.Api.Domain.Models;

public sealed record Widget(
    Guid Id,
    Guid TenantId,
    string Name,
    string Type,
    string Title,
    string ButtonText,
    DateTimeOffset CreatedAt);
