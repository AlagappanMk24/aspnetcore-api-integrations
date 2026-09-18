using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UPS.AddressValidation.Infrastructure.Configuration;

namespace UPS.AddressValidation.Infrastructure.Authentication;

public sealed class UpsTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<UpsOptions> options,
    ILogger<UpsTokenProvider> logger)
    : IUpsTokenProvider
{
    private readonly UpsOptions _options = options.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _accessToken;
    private DateTimeOffset _expiresAtUtc;

    public async Task<string> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        if (HasUsableToken())
        {
            return _accessToken!;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (HasUsableToken())
            {
                return _accessToken!;
            }

            return await RequestTokenAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private bool HasUsableToken() =>
        !string.IsNullOrWhiteSpace(_accessToken)
        && DateTimeOffset.UtcNow < _expiresAtUtc;

    private async Task<string> RequestTokenAsync(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException(
                "UPS ClientId and ClientSecret are required.");
        }

        var client = httpClientFactory.CreateClient("UpsOAuth");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/security/v1/oauth/token");

        var credentials =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_options.ClientId}:{_options.ClientSecret}"));

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>(
                "grant_type",
                "client_credentials")
        ]);

        using var response =
            await client.SendAsync(request, cancellationToken);

        var content =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "UPS OAuth token request failed. HTTP {StatusCode}.",
                (int)response.StatusCode);

            throw new HttpRequestException(
                $"UPS OAuth token request failed with HTTP {(int)response.StatusCode}.");
        }

        var token =
            JsonSerializer.Deserialize<UpsOAuthTokenResponse>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (token is null ||
            string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException(
                "UPS OAuth response did not contain an access token.");
        }

        var expiresIn =
            token.ExpiresIn > 0
                ? token.ExpiresIn
                : 14400;

        // Refresh one minute before the actual expiry.
        var refreshSeconds = Math.Max(60, expiresIn - 60);

        _accessToken = token.AccessToken;
        _expiresAtUtc =
            DateTimeOffset.UtcNow.AddSeconds(refreshSeconds);

        return _accessToken;
    }

    private sealed record UpsOAuthTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
