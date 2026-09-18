using FedEx.Tracking.Domain.Models;
namespace FedEx.Tracking.Application.Abstractions;
public interface IFedExTrackingService 
{ 
    Task<IReadOnlyList<TrackingResult>> TrackAsync(IReadOnlyCollection<string> numbers,bool detailed,CancellationToken ct); 
}