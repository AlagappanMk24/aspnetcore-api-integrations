namespace Vertex.Tax.Infrastructure.Configuration;

public sealed class VertexOptions
{
    public const string SectionName = "Vertex";
    public string AuthBaseUrl { get; init; } = "https://auth.vertexcloud.com";
    public string ApiBaseUrl { get; init; } = "https://developer.vertexcloud.com";
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Audience { get; init; } = "verx://migration-api";
    public int TokenSafetySeconds { get; init; } = 60;
}