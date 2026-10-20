using System.Collections.Concurrent;
using Flyrank.WidgetPlatform.Api.Domain.Models;

namespace Flyrank.WidgetPlatform.Api.Data;

public sealed class InMemoryWidgetPlatformStore : IWidgetPlatformStore
{
    private readonly ConcurrentDictionary<Guid, Widget> _widgets = new();
    private readonly ConcurrentDictionary<Guid, Submission> _submissions = new();

    public IReadOnlyList<Widget> GetWidgets(Guid tenantId) =>
        _widgets.Values
            .Where(widget => widget.TenantId == tenantId)
            .OrderByDescending(widget => widget.CreatedAt)
            .ToArray();

    public Widget? GetWidget(Guid tenantId, Guid widgetId) =>
        _widgets.TryGetValue(widgetId, out var widget) && widget.TenantId == tenantId
            ? widget
            : null;

    public Widget? GetWidgetById(Guid widgetId) =>
        _widgets.TryGetValue(widgetId, out var widget) ? widget : null;

    public Widget AddWidget(Widget widget)
    {
        if (!_widgets.TryAdd(widget.Id, widget))
        {
            throw new InvalidOperationException("A widget with this identifier already exists.");
        }

        return widget;
    }

    public bool UpdateWidget(Widget widget) =>
        _widgets.TryGetValue(widget.Id, out var existing)
        && existing.TenantId == widget.TenantId
        && _widgets.TryUpdate(widget.Id, widget, existing);

    public bool DeleteWidget(Guid tenantId, Guid widgetId) =>
        _widgets.TryGetValue(widgetId, out var widget)
        && widget.TenantId == tenantId
        && _widgets.TryRemove(widgetId, out _);

    public bool WidgetExists(Guid widgetId) => _widgets.ContainsKey(widgetId);

    public Submission AddSubmission(Submission submission)
    {
        if (!_submissions.TryAdd(submission.Id, submission))
        {
            throw new InvalidOperationException("A submission with this identifier already exists.");
        }

        return submission;
    }

    public IReadOnlyList<Submission> GetSubmissions(Guid widgetId) =>
        _submissions.Values
            .Where(submission => submission.WidgetId == widgetId)
            .OrderByDescending(submission => submission.CreatedAt)
            .ToArray();
}
