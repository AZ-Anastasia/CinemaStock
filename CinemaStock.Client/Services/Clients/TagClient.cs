using CinemaStock.Client.Services.Clients.Base;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Routes;

namespace CinemaStock.Client.Services.Clients;

public class TagClient : ApiHttpClient, ITagService
{
    public TagClient(HttpClient http) : base(http) { }

    public async Task<List<IdNameResponse>> GetTagsListAsync()
    {
        return await GetAsync<List<IdNameResponse>>(ApiRoutes.Admin.TagsList);
    }

    public async Task<IdNameResponse?> CreateTagAsync(CreateGenreOrTagRequest request)
    {
        return await PostAsync<CreateGenreOrTagRequest, IdNameResponse>(ApiRoutes.Admin.TagsControllerName, request);
    }

    public async Task<IdNameResponse?> UpdateTagAsync(Guid id, GenreTagRequest request)
    {
        return await PutAsync<GenreTagRequest, IdNameResponse>($"{ApiRoutes.Admin.TagsControllerName}/{id}", request);
    }

    public async Task<bool> DeleteTagAsync(Guid id)
    {
        return await DeleteAsync($"{ApiRoutes.Admin.TagsControllerName}/{id}");
    }
}