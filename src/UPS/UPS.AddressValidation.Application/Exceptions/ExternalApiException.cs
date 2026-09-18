namespace UPS.AddressValidation.Application.Exceptions;

public sealed class ExternalApiException(
    string message,
    int statusCode,
    IReadOnlyCollection<(string Code, string Message)>? errors = null,
    Exception? innerException = null) : Exception(message, innerException)
{
    public int StatusCode { get; } = statusCode;

    public IReadOnlyCollection<(string Code, string Message)> Errors { get; } = errors ?? [];
}
