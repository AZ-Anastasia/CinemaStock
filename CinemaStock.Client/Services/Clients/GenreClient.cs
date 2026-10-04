using CinemaStock.Client.Services.Clients.Base;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Routes;

namespace CinemaStock.Client.Services.Clients;

public class GenreClient : ApiHttpClient, IGenreService
{
    public GenreClient(HttpClient http) : base(http) { }

    public async Task<List<IdNameResponse>> GetGenresListAsync()
    {
        return await GetAsync<List<IdNameResponse>>(ApiRoutes.Admin.GenresList);
    }

    public async Task<IdNameResponse?> CreateGenreAsync(CreateGenreOrTagRequest request)
    {
        return await PostAsync<CreateGenreOrTagRequest, IdNameResponse>(ApiRoutes.Admin.GenresControllerName, request);
    }

    public async Task<IdNameResponse?> UpdateGenreAsync(Guid id, GenreTagRequest request)
    {
        return await PutAsync<GenreTagRequest, IdNameResponse>($"{ApiRoutes.Admin.GenresControllerName}/{id}", request);
    }

    public async Task<bool> DeleteGenreAsync(Guid id)
    {
        return await DeleteAsync($"{ApiRoutes.Admin.GenresControllerName}/{id}");
    }
}