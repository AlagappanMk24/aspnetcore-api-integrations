using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UPS.AddressValidation.Application.Contracts;
using UPS.AddressValidation.Infrastructure.Authentication;
using UPS.AddressValidation.Infrastructure.Configuration;
using UPS.AddressValidation.Infrastructure.Services;

namespace UPS.AddressValidation.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<UpsOptions>()
            .Bind(configuration.GetSection(UpsOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.BaseUrl,
                    UriKind.Absolute,
                    out _),
                "Ups:BaseUrl must be a valid absolute URI.")
            .Validate(
                options => Uri.TryCreate(
                    options.OAuthBaseUrl,
                    UriKind.Absolute,
                    out _),
                "Ups:OAuthBaseUrl must be a valid absolute URI.")
            .Validate(
                options => options.TimeoutSeconds > 0,
                "Ups:TimeoutSeconds must be greater than zero.");

        services.AddHttpClient(
            "UpsOAuth",
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<IOptions<UpsOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(options.OAuthBaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        options.TimeoutSeconds);
            });

        services.AddHttpClient(
            "UpsApi",
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<IOptions<UpsOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(options.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        options.TimeoutSeconds);
            });

        services.AddSingleton<IUpsTokenProvider, UpsTokenProvider>();
        services.AddScoped<IAddressValidationService, UpsAddressValidationService>();

        return services;
    }
}
