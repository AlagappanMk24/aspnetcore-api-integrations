namespace FedEx.Tracking.Domain.Models;
public sealed record TrackingEvent(
    string? EventType,
    string? Description,
    DateTimeOffset? EventTime,
    string? City,
    string? StateOrProvinceCode,
    string? CountryCode);
public sealed record TrackingResult(
    string TrackingNumber,
    string? Status,
    string? StatusDescription,
    string? EstimatedDelivery,
    IReadOnlyList<TrackingEvent> Events);