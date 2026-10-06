using PackingSlip.Pdf.Domain.Models;

namespace PackingSlip.Pdf.Application.Contracts;

public interface IPackingSlipPdfGenerator
{
    byte[] Generate(Domain.Models.PackingSlip packingSlip);
}
