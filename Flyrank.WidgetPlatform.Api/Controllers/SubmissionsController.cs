using Flyrank.WidgetPlatform.Api.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Flyrank.WidgetPlatform.Api.Controllers;

[ApiController]
[Route("api/submissions")]
public sealed class SubmissionsController(ISubmissionService submissionService) : ControllerBase
{
    /// <summary>Accepts a public widget submission.</summary>
    [HttpPost]
    [RequestSizeLimit(65536)]
    [EnableCors("widget-embed")]
    [EnableRateLimiting("public-submissions")]
    [ProducesResponseType<SubmissionReceiptResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SubmissionReceiptResponse>> Create(
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var submission = await submissionService.CreateAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request,
            cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        return Accepted(new SubmissionReceiptResponse(submission.Id, true));
    }
}
