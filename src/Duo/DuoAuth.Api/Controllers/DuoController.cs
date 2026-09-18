using DuoAuth.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DuoAuth.Api.Controllers;

[ApiController]
[Route("api/duo")]
public sealed class DuoController(
    IDuoAuthService duoAuthService,
    ILogger<DuoController> logger) : ControllerBase
{
    private const string DuoStateSessionKey = "duo_state";
    private const string DuoUsernameSessionKey = "duo_username";

    // Call only after your own username/password or identity-provider authentication succeeds.
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartDuoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Start(
        [FromBody] StartDuoRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { Message = "Username is required." });
        }

        var stateBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        var state = Convert.ToBase64String(stateBytes);

        // Store both state and username for the callback
        HttpContext.Session.SetString(DuoStateSessionKey, state);
        HttpContext.Session.SetString(DuoUsernameSessionKey, request.Username);

        try
        {
            var authUrl = await duoAuthService.CreateAuthorizationUrlAsync(request.Username, state, cancellationToken);

            return Ok(new StartDuoResponse(authUrl));
        }
        catch (Exception ex)
        {
            // Clean up session on failure
            HttpContext.Session.Remove(DuoStateSessionKey);
            HttpContext.Session.Remove(DuoUsernameSessionKey);
            logger.LogError(ex, "Could not start Duo authentication for user {Username}", request.Username);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { Message = "Unable to start Duo authentication." });
        }
    }

    [HttpGet("callback")]
    [ProducesResponseType(typeof(DuoCallbackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Callback(
        [FromQuery(Name = "duo_code")] string? duoCode,
        [FromQuery(Name = "state")] string? state,
        CancellationToken cancellationToken)
    {
        var expectedState = HttpContext.Session.GetString(DuoStateSessionKey);
        var username = HttpContext.Session.GetString(DuoUsernameSessionKey);

        // Always clean up
        HttpContext.Session.Remove(DuoStateSessionKey);
        HttpContext.Session.Remove(DuoUsernameSessionKey);

        var result = await duoAuthService.VerifyCallbackAsync(
            duoCode ?? string.Empty,
            state ?? string.Empty,
            expectedState ?? string.Empty,
            username ?? string.Empty,
            cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(new { Message = "Duo authentication failed." });
        }

        // IMPORTANT: issue your application's own short-lived session/JWT here after mapping result.Username
        // to the already-primary-authenticated local identity. Do not treat Duo as primary authentication.
        return Ok(new DuoCallbackResponse(
            Authenticated: true,
            Username: result.Username,
            Message: "Duo MFA verified; application token issuance is intentionally delegated."));
    }
}

// ==========================================
// DTO Contracts
// ==========================================

public sealed record StartDuoRequest(string Username);

public sealed record StartDuoResponse(string AuthorizationUrl);

public sealed record DuoCallbackResponse(
    bool Authenticated,
    string Username,
    string Message);