using Microsoft.Extensions.DependencyInjection;
using Vertex.Tax.Application.Validators;

namespace Vertex.Tax.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVertexApplication(this IServiceCollection services)
    {
        services.AddScoped<TaxCalculationValidator>();
        return services;
    }
}