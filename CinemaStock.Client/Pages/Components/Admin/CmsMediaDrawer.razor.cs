using System.Net.Http.Json;
using CinemaStock.Client.Services.Clients;
using CinemaStock.Client.Services.Upload;
using CinemaStock.Client.Validation;
using CinemaStock.Shared;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

namespace CinemaStock.Client.Pages.Components.Admin;

public partial class CmsMediaDrawer : ComponentBase, IAsyncDisposable
{
    public string localhostApi { get; set; } = null!;

    [Inject] private IClipboardUploadService _uploadService { get; set; } = null!;
    [Inject] private IImageValidator _imageValidator { get; set; } = null!;

    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public CreateCinemaContentRequest RequestModel { get; set; } = new();
    [Parameter] public FileUploadOptions Options { get; set; } = new(5 * 1024 * 1024, [".jpg", ".jpeg", ".png", ".webp"]);

    public record FileUploadOptions(long MaxSizeBytes, string[] AllowedExtensions);

    protected ElementReference dropArea;
    protected InputFile? inputFile;

    protected string? errorMessage;
    protected string? previewUrl; // Временная blob-ссылка для браузера
    private IBrowserFile? fileToUpload;
    private DotNetObjectReference<CmsMediaDrawer>? _dotNetRef = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (dropArea.Context != null)
        {
            // Передаем в сервис зону и лямбду-колбэк, которая выполнится при вставке файла
            await _uploadService.InitializeAsync(dropArea, OnClipboardFileReceived);
        }
    }

    // Логика обработки перехваченного файла стала тривиальной и чистой
    private void OnClipboardFileReceived(string base64Data, string fileName)
    {
        int commaIndex = base64Data.IndexOf(',');
        string base64Raw = commaIndex != -1 ? base64Data.Substring(commaIndex + 1) : base64Data;
        var fileBytes = Convert.FromBase64String(base64Raw);

        // Делегируем валидацию выделенному сервису
        var validation = _imageValidator.ValidateHeaderAndSize(fileBytes, fileName, Options.MaxSizeBytes, Options.AllowedExtensions);

        if (!validation.IsValid)
        {
            errorMessage = validation.ErrorMessage;
            StateHasChanged();
            return;
        }

        errorMessage = null;
        RequestModel.Picture = base64Data;
        // previewUrl = $"data:image/png;base64,{base64Data}";
        previewUrl = base64Data;
        fileToUpload = null;

        StateHasChanged();
    }

    protected async Task TriggerUpload()
    {
        if (inputFile?.Element != null)
        {
            await _uploadService.OpenExplorerAsync(inputFile.Element.Value);
        }
    }

    protected async Task ExecuteSave()
    {
        try
        {
            if (DateOnly.TryParse(_releaseDateText, out var parsedDate))
            {
                if (parsedDate.Year < 1800 || parsedDate.Year > 2100)
                    RequestModel.ReleaseDate = null;
                else
                    RequestModel.ReleaseDate = parsedDate;
            }
            else
                RequestModel.ReleaseDate = null;

            var result = await CinemaContentClient.CreateOrUpdateCinemaContentAsync(
                RequestModel,
                fileToUpload,
                Options.MaxSizeBytes,
                EditingMediaId,
                IsEditMode
            );

            if (result != null)
            {
                await OnSaved.InvokeAsync();
            }

            // var httpMethod = IsEditMode ? HttpMethod.Put : HttpMethod.Post;

            // var request = new HttpRequestMessage(httpMethod, targetUrl)
            // {
            //     Content = content
            // };
            // request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            // response = await Http.SendAsync(request);

            // if (response.IsSuccessStatusCode)
            //     await OnSaved.InvokeAsync();
            // else
            //     Logger.LogError("Ошибка сервера при сохранении релиза. Статус: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Критический сбой фронтенда при попытке сохранить релиз.");
        }
    }

    protected async Task CloseDrawer()
    {
        _isGenrePopupOpen = false;
        _isTagPopupOpen = false;
        _isDatePickerOpen = false;

        // Очищаем JS-слушатели через инфраструктурный сервис
        // await _uploadService.DisposeHandlerAsync();
        if (jsModule != null && dropArea.Context != null)
            await jsModule.InvokeVoidAsync("dispose", dropArea);

        if (!string.IsNullOrEmpty(previewUrl) && jsModule != null)
        {
            await jsModule.InvokeVoidAsync("revokePreviewUrl", previewUrl);
            previewUrl = null;
        }

        fileToUpload = null;
        errorMessage = null;
        // previewUrl = null;

        await OnClose.InvokeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (jsModule != null && dropArea.Context != null)
        {
            await jsModule.InvokeVoidAsync("dispose", dropArea);
            await jsModule.DisposeAsync();
        }
        // if (jsModule != null) await jsModule.DisposeAsync();
        _dotNetRef?.Dispose();
    }

    [Inject] private IConfiguration Configuration { get; set; } = null!;

    protected string ResolveImageUrl()
    {
        string? targetPath = !string.IsNullOrEmpty(previewUrl) ? previewUrl : RequestModel.Picture;

        if (string.IsNullOrEmpty(targetPath))
            return ConstStrings.DefaultFilmImagePath;

        var trimmedPath = targetPath.Trim();
        if (trimmedPath.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            return trimmedPath;

        var baseUri = Configuration.GetSection("BackendSettings")["ApiBaseUrl"]?.TrimEnd('/') ?? string.Empty;

        if (string.IsNullOrEmpty(baseUri))
        {
            Logger.LogError("Критическая ошибка: все еще не подхватывается апи локалхост {baseUri}", baseUri);
            // baseUri = ApiHttp.Client.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
            baseUri = Http.BaseAddress?.ToString() ?? string.Empty;
        }
            Logger.LogError("Критическая ошибка: все еще не подхватывается апи локалхост {baseUri}", baseUri);
        // var baseUri = Http.BaseAddress?.ToString().TrimEnd('/');
        var cleanRelativeUrl = trimmedPath.TrimStart('/');

        return $"{baseUri}/{cleanRelativeUrl}";
    }

    [Inject] public HttpClient Http { get; set; } = null!;

    protected async Task<List<IdNameResponse>> FetchDataFromApi(string controllerName)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{controllerName}/{ApiRoutes.Admin.ListEndpoint}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await Http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<IdNameResponse>>() ?? [];
            }

            Logger.LogWarning("Сервер вернул ошибку при загрузке из {Controller}: {StatusCode}",
            controllerName,
            response.StatusCode);
            return [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Ошибка при запросе к контроллеру {Controller}", controllerName);
            return [];
        }
    }
}