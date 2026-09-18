using FedEx.Tracking.Application.Abstractions;
using FedEx.Tracking.Domain.Models;
using FluentValidation;
using MediatR;

namespace FedEx.Tracking.Application.Tracking;
public sealed record TrackShipmentsQuery(
    IReadOnlyCollection<string> TrackingNumbers,
    bool IncludeDetailedScans) : IRequest<IReadOnlyList<TrackingResult>>;
public sealed class TrackShipmentsQueryValidator : AbstractValidator<TrackShipmentsQuery>
{
    public TrackShipmentsQueryValidator()
    {
        RuleFor(x => x.TrackingNumbers).NotNull().Must(x => x is { Count: >= 1 and <= 30 }).WithMessage("Provide 1–30 tracking numbers.");
        RuleForEach(x => x.TrackingNumbers).NotEmpty().MaximumLength(40);
    }
}
public sealed class TrackShipmentsQueryHandler(IFedExTrackingService service) : IRequestHandler<TrackShipmentsQuery, IReadOnlyList<TrackingResult>>
{
    public Task<IReadOnlyList<TrackingResult>> Handle(TrackShipmentsQuery request, CancellationToken ct) 
        => service.TrackAsync(request.TrackingNumbers, request.IncludeDetailedScans, ct);
}