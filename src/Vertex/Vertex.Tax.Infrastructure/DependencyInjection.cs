using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vertex.Tax.Application.Contracts;
using Vertex.Tax.Infrastructure.Authentication;
using Vertex.Tax.Infrastructure.Configuration;
using Vertex.Tax.Infrastructure.Vertex;

namespace Vertex.Tax.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVertexInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<VertexOptions>()
            .Bind(configuration.GetSection(VertexOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.AuthBaseUrl, UriKind.Absolute, out _), "Invalid AuthBaseUrl")
            .Validate(options => Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out _), "Invalid ApiBaseUrl")
            .ValidateOnStart();

        services.AddHttpClient("VertexAuth", (serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VertexOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.AuthBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("VertexApi", (serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VertexOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.ApiBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddSingleton<VertexTokenProvider>();
        services.AddScoped<ITaxCalculator, VertexTaxCalculator>();

        return services;
    }
}