using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UPS.AddressSuggestion.Infrastructure.Configuration;
using UPS.AddressSuggestion.Infrastructure.Exceptions;

namespace UPS.AddressSuggestion.Infrastructure.Authentication;

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
            return _accessToken!;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (HasUsableToken())
                return _accessToken!;

            if (string.IsNullOrWhiteSpace(_options.ClientId) ||
                string.IsNullOrWhiteSpace(_options.ClientSecret))
            {
                throw new InvalidOperationException(
                    "UPS ClientId and ClientSecret must be configured.");
            }

            var client = httpClientFactory.CreateClient("UpsOAuth");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/security/v1/oauth/token");

            var basic = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_options.ClientId}:{_options.ClientSecret}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", basic);

            request.Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>(
                    "grant_type",
                    "client_credentials")
            ]);

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var body = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "UPS OAuth token request failed with status {StatusCode}.",
                    (int)response.StatusCode);

                throw new UpsApiException(
                    (int)response.StatusCode,
                    "Unable to obtain UPS OAuth access token.");
            }

            var token = JsonSerializer.Deserialize<UpsTokenResponse>(
                body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (string.IsNullOrWhiteSpace(token?.AccessToken))
            {
                throw new UpsApiException(
                    502,
                    "UPS OAuth response did not contain an access token.");
            }

            _accessToken = token.AccessToken;
            _expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(60, token.ExpiresIn));

            return _accessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private bool HasUsableToken() =>
        !string.IsNullOrWhiteSpace(_accessToken) &&
        DateTimeOffset.UtcNow < _expiresAtUtc.AddSeconds(-60);

    private sealed class UpsTokenResponse
    {
        public string? AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
