using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FedEx.Tracking.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FedEx.Tracking.Infrastructure.Authentication;

public interface IFedExTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}

public sealed class FedExTokenService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<FedExOptions> options) : IFedExTokenService
{
    private const string CacheKey = "fedex-token";

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out string? cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        var fedExOptions = options.Value;

        if (string.IsNullOrWhiteSpace(fedExOptions.ClientId) || string.IsNullOrWhiteSpace(fedExOptions.ClientSecret))
        {
            throw new InvalidOperationException("FedEx ClientId and ClientSecret are not configured.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{fedExOptions.BaseUrl.TrimEnd('/')}/oauth/token")
        {
            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("grant_type", "client_credentials"),
                new("client_id", fedExOptions.ClientId),
                new("client_secret", fedExOptions.ClientSecret)
            })
        };

        var client = httpClientFactory.CreateClient("FedExAuth");
        using var response = await client.SendAsync(request, cancellationToken);

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
        {
            throw new HttpRequestException($"FedEx OAuth failed with status code: {(int)response.StatusCode}");
        }

        // Cache token with a safety buffer (expire 2 minutes early)
        var cacheExpiration = TimeSpan.FromSeconds(Math.Max(60, tokenResponse.ExpiresIn - 120));
        cache.Set(CacheKey, tokenResponse.AccessToken, cacheExpiration);

        return tokenResponse.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}