namespace DuoAuth.Api.Services;

public interface IDuoAuthService
{
    Task<string> CreateAuthorizationUrlAsync(
        string username,
        string state,
        CancellationToken cancellationToken);

    Task<DuoVerificationResult> VerifyCallbackAsync(
            string code,
            string state,
            string expectedState,
            string username,
            CancellationToken cancellationToken);
}

public sealed record DuoVerificationResult(
    bool Success,
    string? Username,
    string? Reason);