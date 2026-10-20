namespace Flyrank.WidgetPlatform.Api.Services;

public interface IWidgetService
{
    IReadOnlyList<WidgetResponse> GetAll(Guid tenantId);
    WidgetResponse? GetById(Guid tenantId, Guid widgetId);
    WidgetResponse Create(CreateWidgetRequest request);
    WidgetResponse? Update(Guid widgetId, UpdateWidgetRequest request);
    bool Delete(Guid tenantId, Guid widgetId);
    WidgetConfigurationResponse? GetConfiguration(Guid widgetId);
    WidgetEmbedResponse? GetEmbed(Guid widgetId, string? publicBaseUrl);
    string? GetLoaderScript(Guid widgetId, string? publicBaseUrl);
}
