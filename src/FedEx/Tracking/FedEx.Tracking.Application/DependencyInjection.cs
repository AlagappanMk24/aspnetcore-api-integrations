using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FedEx.Tracking.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddTrackingApplication(this IServiceCollection service)
    {
        service.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        service.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return service;
    }
}