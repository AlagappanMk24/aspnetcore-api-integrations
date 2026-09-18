namespace FedEx.Tracking.Infrastructure.Configuration;
public sealed class FedExOptions
{
    public const string SectionName = "FedEx";
    public string BaseUrl { get; set; } = "https://apis-sandbox.fedex.com";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}