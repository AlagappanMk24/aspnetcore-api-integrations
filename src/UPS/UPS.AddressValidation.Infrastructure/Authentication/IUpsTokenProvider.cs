namespace UPS.AddressValidation.Infrastructure.Authentication;

public interface IUpsTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
