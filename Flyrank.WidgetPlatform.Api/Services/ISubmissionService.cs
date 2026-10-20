using Flyrank.WidgetPlatform.Api.Domain.Models;

namespace Flyrank.WidgetPlatform.Api.Services;

public interface ISubmissionService
{
    Task<Submission?> CreateAsync(
        CreateSubmissionRequest request,
        string? ipAddress,
        Microsoft.AspNetCore.Http.HttpRequest httpRequest,
        CancellationToken cancellationToken);

    IReadOnlyList<SubmissionResponse> GetForWidget(Guid widgetId);
    WidgetDashboardResponse? GetDashboard(Guid tenantId, Guid widgetId);
}
