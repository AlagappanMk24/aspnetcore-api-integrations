namespace UPS.AddressSuggestion.Infrastructure.Authentication;

public interface IUpsTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
