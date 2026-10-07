using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PackingSlip.Pdf.Application.Contracts;
using PackingSlip.Pdf.Application.Services;
using PackingSlip.Pdf.Domain.Models;

namespace PackingSlip.Pdf.Api.Controllers;

[ApiController]
[Route("api/packing-slips")]
public sealed class PackingSlipController(IPackingSlipPdfGenerator generator, ITemplatePackingSlipPdfGenerator templateGenerator) : ControllerBase
{
    private readonly IPackingSlipPdfGenerator _generator = generator;
    private readonly ITemplatePackingSlipPdfGenerator _templateGenerator = templateGenerator;
    [HttpPost("pdf")]
    [Produces("application/pdf")]
    public IActionResult GeneratePdf([FromBody] Domain.Models.PackingSlip request)
    {
        var errors = PackingSlipValidator.Validate(request);
        if (errors.Count > 0)
        {
            var details = new ValidationProblemDetails(
                errors
                    .Select((message, index) => new { Key = $"packingSlip_{index}", Message = message })
                    .ToDictionary(x => x.Key, x => new[] { x.Message }));

            return ValidationProblem(details);
        }

        var pdf = _generator.Generate(request);
        var fileName = $"packing-slip-{SanitizeFileName(request.OrderNumber)}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpPost("template-pdf")]
    [Produces("application/pdf")]
    public IActionResult GenerateTemplatePdf([FromBody] TemplatePackingSlip request)
    {
        if (request.Items.Count > 7)
            return BadRequest(new { message = "The supplied commercial template supports a maximum of 7 item rows per page." });

        var pdf = _templateGenerator.Generate(request);
        var fileName = $"commercial-packing-slip-{SanitizeFileName(request.OrderNumber)}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    private static string SanitizeFileName(string value)
        => Regex.Replace(value, @"[^a-zA-Z0-9._-]+", "-").Trim('-');
}
