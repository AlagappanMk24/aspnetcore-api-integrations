using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FedEx.Tracking.Application.Abstractions;
using FedEx.Tracking.Domain.Models;
using FedEx.Tracking.Infrastructure.Authentication;
using FedEx.Tracking.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FedEx.Tracking.Infrastructure.Tracking;
public sealed class FedExTrackingService(IHttpClientFactory factory, IFedExTokenService tokenService, IOptions<FedExOptions> opts, ILogger<FedExTrackingService> logger) : IFedExTrackingService
{
    public async Task<IReadOnlyList<TrackingResult>> TrackAsync(IReadOnlyCollection<string> numbers, bool detailed, CancellationToken ct)
    {
        var token = await tokenService.GetAccessTokenAsync(ct);
        var payload = new Request(detailed, numbers.Select(n => new Info(new NumberInfo(n))).ToArray());

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{opts.Value.BaseUrl.TrimEnd('/')}/track/v1/trackingnumbers")
        {
            Content = JsonContent.Create(payload)
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var res = await factory.CreateClient("FedExApi").SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);

            logger.LogWarning("FedEx tracking returned {Status}: {Body}", (int)res.StatusCode, err);

            throw new HttpRequestException($"FedEx tracking failed: HTTP {(int)res.StatusCode}");
        }

        var data = await res.Content.ReadFromJsonAsync<Response>(cancellationToken: ct);

        return (data?.Output?.CompleteTrackResults ?? Array.Empty<Complete>()).Select(Map).ToArray();
    }
    private static TrackingResult Map(Complete complete)
    {
        var trackResult = complete.TrackResults?.FirstOrDefault();
        var statusDetail = trackResult?.LatestStatusDetail;

        var trackingEvents = (trackResult?.ScanEvents ?? Enumerable.Empty<Scan>())
            .Select(scan => new TrackingEvent(
                scan.EventType,
                scan.EventDescription,
                DateTimeOffset.TryParse(scan.Date, out var parsedDate) ? parsedDate : null,
                scan.ScanLocation?.City,
                scan.ScanLocation?.StateOrProvinceCode,
                scan.ScanLocation?.CountryCode))
            .ToArray();

        var estimatedDelivery = trackResult?.DateAndTimes?
            .FirstOrDefault(dateTime => dateTime.Type == "ESTIMATED_DELIVERY")?
            .DateTime;

        return new TrackingResult(
            complete.TrackingNumberInfo?.TrackingNumber ?? string.Empty,
            statusDetail?.Code,
            statusDetail?.Description,
            estimatedDelivery,
            trackingEvents);
    }

    // ==========================================
    // Request DTOs
    // ==========================================

    private sealed record Request(
        [property: JsonPropertyName("includeDetailedScans")] bool IncludeDetailedScans,
        [property: JsonPropertyName("trackingInfo")] Info[] TrackingInfo);

    private sealed record Info(
        [property: JsonPropertyName("trackingNumberInfo")] NumberInfo TrackingNumberInfo);

    private sealed record NumberInfo(
        [property: JsonPropertyName("trackingNumber")] string TrackingNumber);

    // ==========================================
    // Response DTOs
    // ==========================================

    private sealed record Response(
        [property: JsonPropertyName("output")] Output? Output);

    private sealed record Output(
        [property: JsonPropertyName("completeTrackResults")] Complete[]? CompleteTrackResults);

    private sealed record Complete(
        [property: JsonPropertyName("trackingNumberInfo")] RespNumber? TrackingNumberInfo,
        [property: JsonPropertyName("trackResults")] Detail[]? TrackResults);

    private sealed record RespNumber(
        [property: JsonPropertyName("trackingNumber")] string? TrackingNumber);

    private sealed record Detail(
        [property: JsonPropertyName("latestStatusDetail")] Status? LatestStatusDetail,
        [property: JsonPropertyName("dateAndTimes")] DateTimeItem[]? DateAndTimes,
        [property: JsonPropertyName("scanEvents")] Scan[]? ScanEvents);

    private sealed record Status(
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record DateTimeItem(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("dateTime")] string? DateTime);

    private sealed record Scan(
        [property: JsonPropertyName("eventType")] string? EventType,
        [property: JsonPropertyName("eventDescription")] string? EventDescription,
        [property: JsonPropertyName("date")] string? Date,
        [property: JsonPropertyName("scanLocation")] Location? ScanLocation);

    private sealed record Location(
        [property: JsonPropertyName("city")] string? City,
        [property: JsonPropertyName("stateOrProvinceCode")] string? StateOrProvinceCode,
        [property: JsonPropertyName("countryCode")] string? CountryCode);
  }