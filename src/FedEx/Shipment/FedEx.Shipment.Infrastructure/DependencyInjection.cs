using FedEx.Shipment.Application.Abstractions;
using FedEx.Shipment.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FedEx.Shipment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddShipmentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FedExOptions>(configuration.GetSection("FedEx"));

        services.AddMemoryCache();

        services.AddHttpClient("FedExAuth");
        services.AddHttpClient("FedExApi");

        services.AddScoped<IFedExShipmentService, FedExShipmentService>();

        return services;
    }
}