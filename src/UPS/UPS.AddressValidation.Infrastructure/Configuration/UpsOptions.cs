namespace UPS.AddressValidation.Infrastructure.Configuration;

public sealed class UpsOptions
{
    public const string SectionName = "Ups";

    public string BaseUrl { get; set; } = "https://wwwcie.ups.com/api";

    public string OAuthBaseUrl { get; set; } = "https://wwwcie.ups.com";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    public string TransactionSource { get; set; } = "aspnetcore-api-integrations";
}
