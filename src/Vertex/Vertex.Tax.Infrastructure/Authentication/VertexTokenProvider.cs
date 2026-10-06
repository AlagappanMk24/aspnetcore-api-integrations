using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Vertex.Tax.Infrastructure.Configuration;

namespace Vertex.Tax.Infrastructure.Authentication;
public sealed class VertexTokenProvider(IHttpClientFactory httpClientFactory, IOptions<VertexOptions> vertexOptions, ILogger<VertexTokenProvider> logger)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt;
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_token) && DateTimeOffset.UtcNow < _expiresAt)
        {
            return _token;
        }
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_token) && DateTimeOffset.UtcNow < _expiresAt)
            {
                return _token;
            }
            var options = vertexOptions.Value;
            if (string.IsNullOrWhiteSpace(options.ClientId) || string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                throw new InvalidOperationException("Vertex OAuth credentials are not configured.");
            }
            var httpClient = httpClientFactory.CreateClient("VertexAuth");
            using var request = new HttpRequestMessage(HttpMethod.Post, "/oauth/token")
            {
                Content = JsonContent.Create(new
                {
                    client_id = options.ClientId,
                    client_secret = options.ClientSecret,
                    audience = options.Audience,
                    grant_type = "client_credentials"
                })
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Vertex token request failed with HTTP {StatusCode}", (int)response.StatusCode);
                throw new HttpRequestException("Vertex authentication failed.");
            }
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
                 ?? throw new InvalidOperationException("Empty Vertex token response.");

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("Vertex token response did not contain an access token.");
            }
            _token = tokenResponse.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(30, tokenResponse.ExpiresIn) - Math.Max(15, options.TokenSafetySeconds));

            return _token;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    );
}
