using Flyrank.WidgetPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Flyrank.WidgetPlatform.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(ISubmissionService submissionService) : ControllerBase
{
    /// <summary>Gets submission statistics and recent submissions for a widget.</summary>
    [HttpGet("widgets/{widgetId:guid}")]
    [ProducesResponseType<WidgetDashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<WidgetDashboardResponse> GetWidgetDashboard(
        Guid widgetId,
        [FromQuery, NotEmptyGuid] Guid tenantId)
    {
        var dashboard = submissionService.GetDashboard(tenantId, widgetId);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }
}
