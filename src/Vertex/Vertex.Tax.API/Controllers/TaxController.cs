using Microsoft.AspNetCore.Mvc;
using Vertex.Tax.Application.Contracts;
using Vertex.Tax.Application.Validators;
using Vertex.Tax.Domain.Models;

namespace Vertex.Tax.Api.Controllers;

[ApiController]
[Route("api/tax")]
public sealed class TaxController(
    ITaxCalculator taxCalculator,
    TaxCalculationValidator validator) : ControllerBase
{
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(
        [FromBody] TaxCalculation request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()
                );

            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var result = await taxCalculator.CalculateAsync(request, cancellationToken);
        return Ok(result);
    }
}