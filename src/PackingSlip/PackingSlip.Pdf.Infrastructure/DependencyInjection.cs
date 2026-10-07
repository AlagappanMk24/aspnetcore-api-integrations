using Microsoft.Extensions.DependencyInjection;
using PackingSlip.Pdf.Application.Contracts;
using PackingSlip.Pdf.Infrastructure.Pdf;
using QuestPDF.Infrastructure;

namespace PackingSlip.Pdf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPackingSlipPdfInfrastructure(this IServiceCollection services, LicenseType licenseType)
    {
        QuestPDF.Settings.License = licenseType;
        services.AddSingleton<IPackingSlipPdfGenerator, QuestPdfPackingSlipGenerator>();
        services.AddSingleton<ITemplatePackingSlipPdfGenerator, TemplatePackingSlipPdfGenerator>();
        return services;
    }
}
