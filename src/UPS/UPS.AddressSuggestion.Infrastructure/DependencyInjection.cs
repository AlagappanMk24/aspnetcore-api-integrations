using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UPS.AddressSuggestion.Application.Contracts;
using UPS.AddressSuggestion.Infrastructure.Authentication;
using UPS.AddressSuggestion.Infrastructure.Configuration;
using UPS.AddressSuggestion.Infrastructure.Services;

namespace UPS.AddressSuggestion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUpsAddressSuggestion(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<UpsOptions>()
            .Bind(configuration.GetSection(UpsOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                "Ups:BaseUrl must be a valid absolute URI.")
            .Validate(o => Uri.TryCreate(o.OAuthBaseUrl, UriKind.Absolute, out _),
                "Ups:OAuthBaseUrl must be a valid absolute URI.")
            .ValidateOnStart();

        services.AddHttpClient("UpsOAuth", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<UpsOptions>>().Value;
            client.BaseAddress = new Uri(options.OAuthBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddHttpClient("UpsApi", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<UpsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddSingleton<IUpsTokenProvider, UpsTokenProvider>();
        services.AddScoped<IAddressSuggestionService, UpsAddressSuggestionService>();

        return services;
    }
}
