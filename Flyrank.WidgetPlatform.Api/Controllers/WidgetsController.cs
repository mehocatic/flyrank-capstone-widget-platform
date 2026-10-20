using Flyrank.WidgetPlatform.Api.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Flyrank.WidgetPlatform.Api.Controllers;

[ApiController]
[Route("api/widgets")]
public sealed class WidgetsController(IWidgetService widgetService) : ControllerBase
{
    /// <summary>Lists widgets belonging to a tenant.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<WidgetResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<WidgetResponse>> GetAll(
        [FromQuery, NotEmptyGuid] Guid tenantId)
    {
        return Ok(widgetService.GetAll(tenantId));
    }

    /// <summary>Gets one widget belonging to a tenant.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<WidgetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<WidgetResponse> GetById(
        Guid id,
        [FromQuery, NotEmptyGuid] Guid tenantId)
    {
        var widget = widgetService.GetById(tenantId, id);
        return widget is null ? NotFound() : Ok(widget);
    }

    /// <summary>Creates a widget.</summary>
    [HttpPost]
    [ProducesResponseType<WidgetResponse>(StatusCodes.Status201Created)]
    public ActionResult<WidgetResponse> Create([FromBody] CreateWidgetRequest request)
    {
        var widget = widgetService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = widget.Id, tenantId = widget.TenantId }, widget);
    }

    /// <summary>Replaces a widget's editable properties.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<WidgetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<WidgetResponse> Update(Guid id, [FromBody] UpdateWidgetRequest request)
    {
        var widget = widgetService.Update(id, request);
        return widget is null ? NotFound() : Ok(widget);
    }

    /// <summary>Deletes a widget belonging to a tenant.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id, [FromQuery, NotEmptyGuid] Guid tenantId)
    {
        return widgetService.Delete(tenantId, id) ? NoContent() : NotFound();
    }

    /// <summary>Gets the public configuration for an embedded widget.</summary>
    [HttpGet("{id:guid}/configuration")]
    [EnableCors("widget-embed")]
    [ProducesResponseType<WidgetConfigurationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<WidgetConfigurationResponse> GetConfiguration(Guid id)
    {
        var configuration = widgetService.GetConfiguration(id);
        return configuration is null ? NotFound() : Ok(configuration);
    }

    /// <summary>Serves the JavaScript loader for an embedded widget.</summary>
    [HttpGet("{id:guid}/loader.js")]
    [EnableCors("widget-embed")]
    [Produces("application/javascript")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult GetLoaderScript(Guid id)
    {
        var script = widgetService.GetLoaderScript(id, $"{Request.Scheme}://{Request.Host}");
        return script is null ? NotFound() : Content(script, "application/javascript");
    }

    /// <summary>Generates an embed script for a widget.</summary>
    [HttpGet("{id:guid}/embed")]
    [ProducesResponseType<WidgetEmbedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<WidgetEmbedResponse> GetEmbed(Guid id)
    {
        var embed = widgetService.GetEmbed(id, $"{Request.Scheme}://{Request.Host}");
        return embed is null ? NotFound() : Ok(embed);
    }
}
