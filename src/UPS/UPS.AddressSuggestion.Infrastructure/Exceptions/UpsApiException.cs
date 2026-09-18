namespace UPS.AddressSuggestion.Infrastructure.Exceptions;

public sealed class UpsApiException(
    int statusCode,
    string message,
    string? upsErrorCode = null)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string? UpsErrorCode { get; } = upsErrorCode;
}
