using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;

namespace CinemaStock.Shared.ApiEndpointControllers;

public interface IGenreService
{
    Task<List<IdNameResponse>> GetGenresListAsync();
    Task<IdNameResponse?> CreateGenreAsync(CreateGenreOrTagRequest request);
    Task<IdNameResponse?> UpdateGenreAsync(Guid id, GenreTagRequest request);
    Task<bool> DeleteGenreAsync(Guid id);
}