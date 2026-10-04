using System.Globalization;
using System.Net.Http.Headers;
using CinemaStock.Client.Services.Clients.Base;
using CinemaStock.Shared;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Enums;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Components.Forms;

namespace CinemaStock.Client.Services.Clients;

public class CinemaContentClient : ApiHttpClient, ICinemaContentService
{
    public CinemaContentClient(HttpClient http) : base(http) { }

    public async Task<CinemaContentResponse> CreateOrUpdateCinemaContentAsync(
        CreateCinemaContentRequest request,
        IBrowserFile? fileToUpload,
        long maxSizeBytes,
        Guid? editingMediaId,
        bool isEditMode = false
        )
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(request.OriginalTitle ?? string.Empty), "OriginalTitle" },
            { new StringContent(request.LocalTitle ?? string.Empty), "LocalTitle" },
            { new StringContent(((int)request.Type!).ToString() ?? ((int)ReleaseType.Unknown).ToString()), "Type" },
            // Чтобы не было отличий с "," и "." - поэтому всегда ставим "."
            { new StringContent(request.Rating?.ToString("0.##",CultureInfo.InvariantCulture) ?? string.Empty), "Rating" },
            { new StringContent(request.ReleaseStudio ?? string.Empty), "ReleaseStudio" },
            { new StringContent(request.SourceInfoFrom ?? string.Empty), "SourceInfoFrom" },
            { new StringContent(request.Description ?? string.Empty), "Description" },
            { new StringContent(request.OwnComment ?? string.Empty), "OwnComment" },
            {
                new StringContent(request.ReleaseDate?.ToString(ConstStrings.DefaultDateFormat) ?? string.Empty),
                "ReleaseDate"
            }
        };

        if (!string.IsNullOrEmpty(request.Picture))
            content.Add(new StringContent(request.Picture), "Picture");
        else
            content.Add(new StringContent(ConstStrings.DefaultFilmImagePath), "Picture");

        if (request.GenresIds != null)
            for (int i = 0; i < request.GenresIds.Count; i++)
                content.Add(new StringContent(request.GenresIds[i].ToString()), "GenresIds");

        if (request.TagsIds != null)
            for (int i = 0; i < request.TagsIds.Count; i++)
                content.Add(new StringContent(request.TagsIds[i].ToString()), "TagsIds");

        if (fileToUpload != null)
        {
            var streamContent = new StreamContent(fileToUpload.OpenReadStream(maxSizeBytes));
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileToUpload.ContentType);
            content.Add(streamContent, "Picture", fileToUpload.Name);
        }

        string targetUrl;

        if (isEditMode)
        {
            if (editingMediaId == null || editingMediaId == Guid.Empty)
            {
                throw new Exception("Критическая ошибка: включен режим редактирования, но ID равен null или пуст.");
            }
            targetUrl = $"{ApiRoutes.Admin.CinemaContentEndpoint}/{editingMediaId.Value}";
        }
        else
        {
            targetUrl = ApiRoutes.Admin.CreateCinemaContent;
        }

        return await PostFormAsync<CinemaContentResponse>(targetUrl, content, isEditMode);
    }

    public async Task<CinemaContentResponse> CreateCinemaContentAsync(CreateCinemaContentRequest request)
    {
        return await CreateOrUpdateCinemaContentAsync(
            request,
            fileToUpload: null,
            maxSizeBytes: 1024*1024,
            editingMediaId: null,
            isEditMode: false);
    }

    public Task<List<CinemaContentResponse>> GetCinemaContentsListAsync(string? filter, int pageNumber = 1, int pageSize = 15)
    {
        var url = $"{ApiRoutes.Admin.GetCinemaContent}?page={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrEmpty(filter))
            url += $"&filter={Uri.EscapeDataString(filter)}";

        return GetAsync<List<CinemaContentResponse>>(url);
    }

    public async Task<bool> DeleteCinemaContentAsync(Guid id)
    {
        var url = $"{ApiRoutes.Admin.CinemaContentEndpoint}/{id}";
        return await DeleteAsync(url);
    }

    public async Task<CinemaContentResponse?> UpdateCinemaContentAsync(Guid id, CreateCinemaContentRequest request)
    {
        var url = $"{ApiRoutes.Admin.UpdateCinemaContent}/{id}";
        return await PutAsync<CreateCinemaContentRequest,CinemaContentResponse>(url, request);
    }

    public async Task<List<CinemaContentResponse>> GetCinemaContentsSearchLiveListAsync(string query, CancellationToken ct)
    {
        try
        {
            var escapedQuery = Uri.EscapeDataString(query);
            return await GetAsync<List<CinemaContentResponse>>($"{ApiRoutes.Admin.GetCinemaSearchLive}?query={escapedQuery}", ct);
        }
        catch (Exception) {
            return new List<CinemaContentResponse>();
        }
    }
}