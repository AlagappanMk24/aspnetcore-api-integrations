using Vertex.Tax.Domain.Models;

namespace Vertex.Tax.Application.Contracts;
public interface ITaxCalculator
{
    Task<TaxCalculationResult> CalculateAsync(TaxCalculation calculation, CancellationToken cancellationToken);
}
