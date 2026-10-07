using PackingSlip.Pdf.Domain.Models;

namespace PackingSlip.Pdf.Application.Contracts;

/// <summary>
/// Generates a PDF by reading the supplied commercial packing-slip PDF template
/// and overlaying the request values onto the template.
/// </summary>
public interface ITemplatePackingSlipPdfGenerator
{
    byte[] Generate(TemplatePackingSlip model);
}
