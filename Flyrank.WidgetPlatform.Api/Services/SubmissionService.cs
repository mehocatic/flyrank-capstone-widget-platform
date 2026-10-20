using System.Net;
using System.Text.Json;
using Flyrank.WidgetPlatform.Api.Data;
using Flyrank.WidgetPlatform.Api.Domain.Models;

namespace Flyrank.WidgetPlatform.Api.Services;

public sealed class SubmissionService(
    IWidgetPlatformStore store,
    IGeoEnrichmentService geoEnrichmentService) : ISubmissionService
{
    public async Task<Submission?> CreateAsync(
        CreateSubmissionRequest request,
        string? ipAddress,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        if (!store.WidgetExists(request.WidgetId))
        {
            return null;
        }

        if (request.VisitorData.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("VisitorData must be a JSON object.");
        }

        var isSpam = !string.IsNullOrWhiteSpace(request.Website);
        GeoData? geoData = null;
        if (!isSpam)
        {
            geoData = await geoEnrichmentService.EnrichAsync(ipAddress, httpRequest, cancellationToken);
        }

        var submission = new Submission(
            Guid.NewGuid(),
            request.WidgetId,
            request.VisitorData.Clone(),
            IPAddress.TryParse(ipAddress, out _) ? ipAddress : null,
            geoData,
            isSpam,
            DateTimeOffset.UtcNow);

        return store.AddSubmission(submission);
    }

    public IReadOnlyList<SubmissionResponse> GetForWidget(Guid widgetId) =>
        store.GetSubmissions(widgetId).Select(ToResponse).ToArray();

    public WidgetDashboardResponse? GetDashboard(Guid tenantId, Guid widgetId)
    {
        if (store.GetWidget(tenantId, widgetId) is null)
        {
            return null;
        }

        var submissions = store.GetSubmissions(widgetId);
        var spamCount = submissions.Count(submission => submission.IsSpam);
        return new WidgetDashboardResponse(
            widgetId,
            submissions.Count,
            spamCount,
            submissions.Count - spamCount,
            submissions.Take(20).Select(ToResponse).ToArray());
    }

    private static SubmissionResponse ToResponse(Submission submission) =>
        new(submission.Id, submission.WidgetId, submission.VisitorData, submission.GeoData,
            submission.IsSpam, submission.CreatedAt);
}
