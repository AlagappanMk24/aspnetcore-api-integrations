namespace DuoAuth.Api.Options;
public sealed class DuoOptions
{
    public string ClientId { get; init; } = "";
    public string ClientSecret { get; init; } = "";
    public string ApiHost { get; init; } = "";
    public string CallbackUrl { get; init; } = "";
    public string RedirectAfterSuccess { get; init; } = "/";
}
