using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vertex.Tax.Application.Contracts;
using Vertex.Tax.Domain.Models;
using Vertex.Tax.Infrastructure.Authentication;

namespace Vertex.Tax.Infrastructure.Vertex;

public sealed class VertexTaxCalculator(
    IHttpClientFactory httpClientFactory,
    VertexTokenProvider tokenProvider) : ITaxCalculator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<TaxCalculationResult> CalculateAsync(TaxCalculation calculation, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        var httpClient = httpClientFactory.CreateClient("VertexApi");

        var payload = new
        {
            Currency = calculation.Currency,
            SaleMessageType = calculation.SaleMessageType,
            DocumentNumber = calculation.DocumentNumber,
            DocumentDate = calculation.DocumentDate.ToString("yyyy-MM-dd"),
            Seller = MapAddress(calculation.Seller),
            Customer = MapAddress(calculation.Customer),
            LineItem = calculation.Lines.Select(line => new
            {
                LineItemNumber = line.LineNumber,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Product = new
                {
                    ProductClass = line.ProductClass,
                    ProductCode = line.ProductCode
                }
            })
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/vertex-ws/v2/supplies")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Vertex tax calculation failed: {(int)response.StatusCode}.");
        }

        return new TaxCalculationResult(null, null, [], rawResponse);
    }

    private static object MapAddress(TaxAddress address) => new
    {
        address.Country,
        address.State,
        address.City,
        address.PostalCode,
        address.StreetAddress1
    };
}