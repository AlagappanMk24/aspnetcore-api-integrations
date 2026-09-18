using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UPS.AddressValidation.Application.Contracts;
using UPS.AddressValidation.Application.DTOs;
using UPS.AddressValidation.Application.Exceptions;
using UPS.AddressValidation.Infrastructure.Authentication;
using UPS.AddressValidation.Infrastructure.Configuration;
using UPS.AddressValidation.Infrastructure.Models;

namespace UPS.AddressValidation.Infrastructure.Services;
public sealed class UpsAddressValidationService(
    IHttpClientFactory httpClientFactory,
    IUpsTokenProvider tokenProvider,
    IOptions<UpsOptions> options,
    ILogger<UpsAddressValidationService> logger)
    : IAddressValidationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly UpsOptions _options = options.Value;

    public async Task<AddressValidationResultDto>  ValidateAsync(
        ValidateAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        var token =
            await tokenProvider.GetAccessTokenAsync(cancellationToken);

        var client =
            httpClientFactory.CreateClient("UpsApi");

        var transactionId =
            Guid.NewGuid().ToString("N");

        var query =
            $"regionalrequestindicator={(command.RegionalRequestIndicator ? "True" : "False")}" +
            $"&maximumcandidatelistsize={command.MaximumCandidateListSize}";

        var url =
            $"/addressvalidation/v2/{command.RequestOption}?{query}";

        var requestBody = new UpsXavRequestWrapper
        {
            XavRequest = new UpsXavRequest
            {
                AddressKeyFormat = new UpsAddressKeyFormat
                {
                    ConsigneeName = command.ConsigneeName,
                    AddressLine = command.AddressLines.ToList(),
                    Region = command.Region,
                    PoliticalDivision2 = command.City,
                    PoliticalDivision1 = command.State,
                    PostcodePrimaryLow = command.PostalCode,
                    PostcodeExtendedLow = command.PostalCodeExtension,
                    Urbanization = command.Urbanization,
                    CountryCode = command.CountryCode.ToUpperInvariant()
                }
            }
        };

        var json =
            JsonSerializer.Serialize(requestBody, JsonOptions);

        using var request =
            new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.TryAddWithoutValidation(
            "transId",
            transactionId);

        request.Headers.TryAddWithoutValidation(
            "transactionSrc",
            _options.TransactionSource);

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        logger.LogInformation(
            "Sending UPS Address Validation request. TransactionId: {TransactionId}, RequestOption: {RequestOption}",
            transactionId,
            command.RequestOption);

        using var response =
            await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw CreateExternalApiException(
                response.StatusCode,
                responseContent);
        }

        UpsXavResponseWrapper? upsResponse;

        try
        {
            upsResponse =
                JsonSerializer.Deserialize<UpsXavResponseWrapper>(
                    responseContent,
                    JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Unable to deserialize UPS Address Validation response.");

            throw new ExternalApiException(
                "UPS returned an invalid JSON response.",
                502,
                innerException: ex);
        }

        if (upsResponse?.XavResponse is null)
        {
            throw new ExternalApiException(
                "UPS returned an empty Address Validation response.",
                502);
        }

        return MapResponse(upsResponse.XavResponse);
    }

    private static AddressValidationResultDto MapResponse(
        UpsXavResponse response)
    {
        var candidates =
            response.Candidate
                .Select(MapCandidate)
                .ToList();

        var alerts =
            response.Response.Alert
                .Select(alert =>
                    new AddressAlertDto(
                        alert.Code,
                        alert.Description))
                .ToList();

        return new AddressValidationResultDto(
            IsValid: response.ValidAddressIndicator is not null,
            IsAmbiguous: response.AmbiguousAddressIndicator is not null,
            HasCandidates: candidates.Count > 0,
            StatusCode: response.Response.ResponseStatus.Code,
            StatusDescription: response.Response.ResponseStatus.Description,
            Classification: MapClassification(
                response.AddressClassification),
            Candidates: candidates,
            Alerts: alerts);
    }

    private static AddressCandidateDto MapCandidate(
        UpsCandidate candidate)
    {
        var address = candidate.AddressKeyFormat;

        return new AddressCandidateDto(
            address.ConsigneeName,
            address.AddressLine,
            address.PoliticalDivision2,
            address.PoliticalDivision1,
            address.PostcodePrimaryLow,
            address.PostcodeExtendedLow,
            address.Urbanization,
            address.CountryCode,
            address.Region,
            MapClassification(candidate.AddressClassification));
    }

    private static AddressClassificationDto? MapClassification(
        UpsClassification? classification)
    {
        return classification is null
            ? null
            : new AddressClassificationDto(
                classification.Code,
                classification.Description);
    }

    private static ExternalApiException CreateExternalApiException(  
        System.Net.HttpStatusCode statusCode,
        string responseContent)
    {
        var errors =
            new List<(string Code, string Message)>();

        try
        {
            var errorResponse =
                JsonSerializer.Deserialize<UpsErrorResponse>(
                    responseContent,
                    JsonOptions);

            if (errorResponse?.Response?.Errors is not null)
            {
                errors.AddRange(
                    errorResponse.Response.Errors.Select(error =>
                        (
                            error.Code ?? "UPS_ERROR",
                            error.Message ?? "UPS API returned an error."
                        )));
            }
        }
        catch (JsonException)
        {
            // Keep the generic HTTP error if UPS did not return
            // the documented error JSON shape.
        }

        var message =
            errors.Count > 0
                ? string.Join("; ", errors.Select(x => x.Message))
                : $"UPS Address Validation failed with HTTP {(int)statusCode}.";

        return new ExternalApiException(
            message,
            (int)statusCode,
            errors);
    }
}