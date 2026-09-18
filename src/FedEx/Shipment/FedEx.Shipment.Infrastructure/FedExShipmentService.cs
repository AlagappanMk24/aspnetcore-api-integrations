using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FedEx.Shipment.Application.Abstractions;
using FedEx.Shipment.Application.Shipments;
using FedEx.Shipment.Domain.Models;
using FedEx.Shipment.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FedEx.Shipment.Infrastructure;

public sealed class FedExShipmentService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<FedExOptions> options) : IFedExShipmentService
{
    private const string CacheKey = "fedex-ship-token";

    public async Task<CreateShipmentResult> CreateAsync(
        CreateShipmentCommand command,
        CancellationToken cancellationToken)
    {
        var fedExOptions = options.Value;

        if (string.IsNullOrWhiteSpace(fedExOptions.ClientId) ||
            string.IsNullOrWhiteSpace(fedExOptions.ClientSecret) ||
            string.IsNullOrWhiteSpace(fedExOptions.AccountNumber))
        {
            throw new InvalidOperationException("FedEx credentials and account number are not properly configured.");
        }

        var accessToken = await GetAccessTokenAsync(fedExOptions, cancellationToken);

        var requestBody = new ShipRequest(
            AccountNumber: new Account(fedExOptions.AccountNumber),
            RequestedShipment: new Shipment(
                Shipper: command.Shipper,
                Recipients: [command.Recipient],
                ServiceType: command.ServiceType,
                PackagingType: command.PackagingType,
                PickupType: command.PickupType,
                Weight: new Weight(command.Weight.Units, command.Weight.Value),
                Dimensions: new Dimensions(
                    command.Dimensions.Length,
                    command.Dimensions.Width,
                    command.Dimensions.Height,
                    command.Dimensions.Units),
                Label: new Label(command.LabelFormat)));

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{fedExOptions.BaseUrl.TrimEnd('/')}/ship/v1/shipments")
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var client = httpClientFactory.CreateClient("FedExApi");
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"FedEx Ship API failed with HTTP {(int)response.StatusCode}: {errorContent}");
        }

        var shipResponse = await response.Content.ReadFromJsonAsync<ShipResponse>(cancellationToken: cancellationToken);

        var pieceResponse = shipResponse?.Output?.TransactionShipments?.FirstOrDefault()?.PieceResponses?.FirstOrDefault();
        var document = pieceResponse?.PackageDocuments?.FirstOrDefault();

        return new CreateShipmentResult(
            TransactionId: shipResponse?.TransactionId,
            TrackingNumber: pieceResponse?.TrackingNumber,
            EncodedLabel: document?.EncodedLabel,
            LabelContentType: document?.ContentType);
    }

    private async Task<string> GetAccessTokenAsync(
        FedExOptions fedExOptions,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken;
        }

        using var tokenRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{fedExOptions.BaseUrl.TrimEnd('/')}/oauth/token")
        {
            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("grant_type", "client_credentials"),
                new("client_id", fedExOptions.ClientId),
                new("client_secret", fedExOptions.ClientSecret)
            })
        };

        var client = httpClientFactory.CreateClient("FedExAuth");
        using var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);

        var result = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode || string.IsNullOrEmpty(result?.AccessToken))
        {
            throw new HttpRequestException("FedEx OAuth token request failed.");
        }

        var cacheExpiration = TimeSpan.FromSeconds(Math.Max(60, result.ExpiresIn - 120));
        cache.Set(CacheKey, result.AccessToken, cacheExpiration);

        return result.AccessToken;
    }

    // ==========================================
    // Internal DTOs for FedEx Ship API Payload
    // ==========================================

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record Account(
        [property: JsonPropertyName("value")] string Value);

    private sealed record ShipRequest(
        [property: JsonPropertyName("accountNumber")] Account AccountNumber,
        [property: JsonPropertyName("requestedShipment")] Shipment RequestedShipment);

    private sealed record Shipment(
        Address Shipper,
        Address[] Recipients,
        string ServiceType,
        string PackagingType,
        string PickupType,
        Weight Weight,
        Dimensions Dimensions,
        Label Label);

    private sealed record Weight(string Units, double Value);

    private sealed record Dimensions(int Length, int Width, int Height, string Units);

    private sealed record Label(
        string ImageType,
        string LabelStockType = "PAPER_4X6");

    private sealed record ShipResponse(
        [property: JsonPropertyName("transactionId")] string? TransactionId,
        [property: JsonPropertyName("output")] Output? Output);

    private sealed record Output(
        [property: JsonPropertyName("transactionShipments")] ShipResult[]? TransactionShipments);

    private sealed record ShipResult(
        [property: JsonPropertyName("pieceResponses")] Piece[]? PieceResponses);

    private sealed record Piece(
        [property: JsonPropertyName("trackingNumber")] string? TrackingNumber,
        [property: JsonPropertyName("packageDocuments")] Doc[]? PackageDocuments);

    private sealed record Doc(
        [property: JsonPropertyName("encodedLabel")] string? EncodedLabel,
        [property: JsonPropertyName("contentType")] string? ContentType);
}