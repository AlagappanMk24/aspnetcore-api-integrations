using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UPS.AddressSuggestion.Application.Contracts;
using UPS.AddressSuggestion.Application.DTOs;
using UPS.AddressSuggestion.Infrastructure.Authentication;
using UPS.AddressSuggestion.Infrastructure.Configuration;
using UPS.AddressSuggestion.Infrastructure.Exceptions;
using UPS.AddressSuggestion.Infrastructure.Models;

namespace UPS.AddressSuggestion.Infrastructure.Services;

public sealed class UpsAddressSuggestionService(
    IHttpClientFactory httpClientFactory,
    IUpsTokenProvider tokenProvider,
    IOptions<UpsOptions> options,
    ILogger<UpsAddressSuggestionService> logger)
    : IAddressSuggestionService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly UpsOptions _options = options.Value;

    public async Task<AddressSuggestionResultDto> SuggestAsync(
        SuggestAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);

        var client = httpClientFactory.CreateClient("UpsApi");

        var requestOption = 1; // Address Validation; candidates are returned for ambiguous addresses.

        var path =
            $"/addressvalidation/v2/{requestOption}" +
            $"?regionalrequestindicator={command.RegionalRequestIndicator.ToString().ToLowerInvariant()}" +
            $"&maximumcandidatelistsize={command.MaximumCandidateListSize}";

        var payload = new UpsXavRequestWrapper
        {
            XavRequest = new UpsXavRequest
            {
                AddressKeyFormat = new UpsAddressKeyFormat
                {
                    ConsigneeName = command.ConsigneeName,
                    AddressLine = command.AddressLines,
                    PoliticalDivision2 = command.City,
                    PoliticalDivision1 = command.State,
                    PostcodePrimaryLow = command.PostalCode,
                    PostcodeExtendedLow = command.PostalCodeExtension,
                    Urbanization = command.Urbanization,
                    Region = command.Region,
                    CountryCode = command.CountryCode.ToUpperInvariant()
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("transId", Guid.NewGuid().ToString("N"));
        request.Headers.Add("transactionSrc", "aspnetcore-api-integrations");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw CreateUpsException((int)response.StatusCode, responseBody);

        var upsResponse =
            JsonSerializer.Deserialize<UpsXavResponseWrapper>(
                responseBody, JsonOptions)
            ?? throw new UpsApiException(
                502,
                "UPS returned an empty or unreadable response.");

        return MapResponse(upsResponse.XavResponse);
    }

    private static AddressSuggestionResultDto MapResponse(UpsXavResponse response)
    {
        var suggestions = response.Candidate
            .Select(candidate =>
            {
                var address = candidate.AddressKeyFormat;

                return new AddressSuggestionCandidateDto(
                    address.ConsigneeName,
                    address.AddressLine ?? [],
                    address.PoliticalDivision2,
                    address.PoliticalDivision1,
                    address.PostcodePrimaryLow,
                    address.PostcodeExtendedLow,
                    address.Urbanization,
                    address.Region,
                    address.CountryCode,
                    candidate.AddressClassification?.Code,
                    candidate.AddressClassification?.Description);
            })
            .ToArray();

        return new AddressSuggestionResultDto(
            IsValidAddress: response.ValidAddressIndicator is not null,
            IsAmbiguous: response.AmbiguousAddressIndicator is not null,
            HasSuggestions: suggestions.Length > 0,
            Suggestions: suggestions,
            Status: new AddressSuggestionStatusDto(
                response.Response.ResponseStatus.Code,
                response.Response.ResponseStatus.Description,
                response.Response.TransactionReference?.CustomerContext),
            Alerts: response.Response.Alert
                .Select(a => new AddressSuggestionAlertDto(a.Code, a.Description))
                .ToArray());
    }

    private static UpsApiException CreateUpsException(
        int statusCode,
        string responseBody)
    {
        try
        {
            var error = JsonSerializer.Deserialize<UpsErrorResponse>(
                responseBody, JsonOptions);

            var first = error?.Response?.Errors?.FirstOrDefault();

            return new UpsApiException(
                statusCode,
                first?.Message ?? "UPS Address Validation request failed.",
                first?.Code);
        }
        catch (JsonException)
        {
            return new UpsApiException(
                statusCode,
                "UPS Address Validation request failed.");
        }
    }
}
