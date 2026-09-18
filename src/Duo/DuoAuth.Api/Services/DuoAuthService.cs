using System.Security.Cryptography;
using System.Text;
using DuoAuth.Api.Options;
using DuoUniversal;
using Microsoft.Extensions.Options;

namespace DuoAuth.Api.Services;

/// <summary>
/// Adapter around Duo's official Universal Prompt SDK.
/// Primary authentication must happen before calling this service.
/// </summary>
public sealed class DuoAuthService(
    IOptions<DuoOptions> options,
    ILogger<DuoAuthService> logger) : IDuoAuthService
{
    private readonly DuoOptions _options = options.Value;

    public async Task<string> CreateAuthorizationUrlAsync(
        string username,
        string state,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);

        var client = BuildDuoClient();

        // Returns bool, not a HealthCheckResponse object
        var isHealthy = await client.DoHealthCheck();
        if (!isHealthy)
        {
            throw new InvalidOperationException("Duo service health check failed.");
        }

        // Correct method name
        return client.GenerateAuthUri(username, state);
    }

    public async Task<DuoVerificationResult> VerifyCallbackAsync(
        string code,
        string state,
        string expectedState,
        string username,                     
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(state) ||
            string.IsNullOrWhiteSpace(expectedState) ||
            string.IsNullOrWhiteSpace(username) ||
            !IsStateValid(state, expectedState))
        {
            return new DuoVerificationResult(
                Success: false,
                Username: null,
                Reason: "Invalid or missing state/code/username.");
        }

        try
        {
            var client = BuildDuoClient();

            // Correct method name + casing. Username is required.
            var idToken = await client.ExchangeAuthorizationCodeFor2faResult(code, username);

            if (idToken is null)
            {
                return new DuoVerificationResult(
                    Success: false,
                    Username: null,
                    Reason: "Duo did not return an authentication result.");
            }

            // Optional extra safety check
            if (!string.Equals(idToken.AuthResult?.Result, "allow", StringComparison.OrdinalIgnoreCase))
            {
                return new DuoVerificationResult(
                    Success: false,
                    Username: null,
                    Reason: idToken.AuthResult?.StatusMsg ?? "Duo authentication was not allowed.");
            }

            return new DuoVerificationResult(
                Success: true,
                Username: idToken.Username ?? idToken.Sub,
                Reason: null);
        }
        catch (DuoException ex)
        {
            logger.LogWarning(ex, "Duo SDK exception occurred during callback verification.");
            return new DuoVerificationResult(
                Success: false,
                Username: null,
                Reason: "Duo verification failed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception during Duo callback processing.");
            return new DuoVerificationResult(
                Success: false,
                Username: null,
                Reason: "An error occurred during authentication.");
        }
    }

    private Client BuildDuoClient()
    {
        return new ClientBuilder(
            _options.ClientId,
            _options.ClientSecret,
            _options.ApiHost,
            _options.CallbackUrl
        ).Build();
    }

    private static bool IsStateValid(string state, string expectedState)
    {
        var stateBytes = Encoding.UTF8.GetBytes(state);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedState);
        return CryptographicOperations.FixedTimeEquals(stateBytes, expectedBytes);
    }
}