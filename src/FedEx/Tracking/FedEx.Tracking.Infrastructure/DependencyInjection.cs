using FedEx.Tracking.Application.Abstractions;
using FedEx.Tracking.Infrastructure.Authentication;
using FedEx.Tracking.Infrastructure.Configuration;
using FedEx.Tracking.Infrastructure.Tracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FedEx.Tracking.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddTrackingInfrastructure(this IServiceCollection service,IConfiguration configuration)
    {
        service.Configure<FedExOptions>(configuration.GetSection(FedExOptions.SectionName));
        service.AddMemoryCache();
        service.AddHttpClient("FedExAuth");
        service.AddHttpClient("FedExApi");
        service.AddSingleton<IFedExTokenService,FedExTokenService>();
        service.AddScoped<IFedExTrackingService,FedExTrackingService>();
        return service;
    }
}