using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;

namespace CinemaStock.Shared.ApiEndpointControllers;

public interface ITagService
{
    Task<List<IdNameResponse>> GetTagsListAsync();
    Task<IdNameResponse?> CreateTagAsync(CreateGenreOrTagRequest request);
    Task<IdNameResponse?> UpdateTagAsync(Guid id, GenreTagRequest request);
    Task<bool> DeleteTagAsync(Guid id);
}