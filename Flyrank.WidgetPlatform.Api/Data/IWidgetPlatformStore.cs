using Flyrank.WidgetPlatform.Api.Domain.Models;

namespace Flyrank.WidgetPlatform.Api.Data;

public interface IWidgetPlatformStore
{
    IReadOnlyList<Widget> GetWidgets(Guid tenantId);
    Widget? GetWidget(Guid tenantId, Guid widgetId);
    Widget? GetWidgetById(Guid widgetId);
    Widget AddWidget(Widget widget);
    bool UpdateWidget(Widget widget);
    bool DeleteWidget(Guid tenantId, Guid widgetId);
    bool WidgetExists(Guid widgetId);
    Submission AddSubmission(Submission submission);
    IReadOnlyList<Submission> GetSubmissions(Guid widgetId);
}
