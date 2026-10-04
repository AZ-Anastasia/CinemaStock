using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;

namespace CinemaStock.Shared.ApiEndpointControllers;

public interface ICinemaContentService
{
    Task<List<CinemaContentResponse>> GetCinemaContentsListAsync(string? filter, int pageNumber = 1, int pageSize = 15);
    Task<List<CinemaContentResponse>> GetCinemaContentsSearchLiveListAsync(string query, CancellationToken ct);
    Task<CinemaContentResponse> CreateCinemaContentAsync(CreateCinemaContentRequest request);
    Task<bool> DeleteCinemaContentAsync(Guid id);
    Task<CinemaContentResponse?> UpdateCinemaContentAsync(Guid id, CreateCinemaContentRequest request);
}