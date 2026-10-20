using Flyrank.WidgetPlatform.Api.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Flyrank.WidgetPlatform.Api.Services;

public interface IGeoEnrichmentService
{
    Task<GeoData?> EnrichAsync(string? ipAddress, HttpRequest request, CancellationToken cancellationToken);
}
