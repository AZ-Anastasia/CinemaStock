using System.Net.Http.Json;
using System.Text.Json;
using CinemaStock.Shared.ApiEndpointControllers;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace CinemaStock.Client.Services.Clients.Base;

public class ApiHttpClient
{
    protected HttpClient _http { get; }
    public ApiHttpClient(HttpClient http) => _http = http;

    protected async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(url, ct);
        return await ProcessResponseAsync<T>(response);
    }

    protected Task PostAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        return SendAsync(request);
    }

    protected Task PostAsync<TRequest>(string url, TRequest value)
        => SendJsonAsync(HttpMethod.Post, url, value);

    protected Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest value)
        => SendJsonAsync<TRequest, TResponse>(HttpMethod.Post, url, value);

    protected Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest value)
        => SendJsonAsync<TRequest, TResponse>(HttpMethod.Put, url, value);
    protected Task PutAsync<TRequest>(string url, TRequest value)
        => SendJsonAsync(HttpMethod.Put, url, value);

    protected async Task<bool> DeleteAsync(string url)
    {
        var response = await _http.DeleteAsync(url);
        await CheckResponseAsync(response);
        return true;
    }

    private async Task<TResponse> SendJsonAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest value)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(value)
        };

        return await SendAsync<TResponse>(request);
    }

    private async Task SendJsonAsync<TRequest>(HttpMethod method, string url, TRequest value)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(value)
        };

        await SendAsync(request);
    }

    protected async Task<bool> SendAsync(HttpRequestMessage request)
    {
        var response = await _http.SendAsync(request);
        await CheckResponseAsync(response);
        return true;
    }



    private async Task<T> SendAsync<T>(HttpRequestMessage request)
    {
        var response = await _http.SendAsync(request);
        return await ProcessResponseAsync<T>(response);
    }

    protected async Task<T> PostFormAsync<T>(string url, MultipartFormDataContent content, bool isEditMode = false)
    {
        var httpMethod = isEditMode ? HttpMethod.Put : HttpMethod.Post;

        var request = new HttpRequestMessage(httpMethod, url)
        {
            Content = content
        };
        WebAssemblyHttpRequestMessageExtensions.SetBrowserRequestCredentials(request, BrowserRequestCredentials.Include);

        var response = await _http.SendAsync(request);
        return await ProcessResponseAsync<T>(response);
    }

    private async Task<HttpResponseMessage> CheckResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API Error: {response.StatusCode}. Детали: {error}");
        }

        return response;
    }
private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        await CheckResponseAsync(response);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var rawContent = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(rawContent))
            throw new InvalidOperationException("Пустой ответ от сервера.");

        try
        {
            using var document = JsonDocument.Parse(rawContent);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object && 
                (root.TryGetProperty("isSuccess", out _) || root.TryGetProperty("IsSuccess", out _)))
            {
                var resultDto = JsonSerializer.Deserialize<ResultDto<T>>(rawContent, options);

                if (resultDto == null)
                    throw new InvalidOperationException("Не удалось распарсить ответ как ResultDto.");

                if (!resultDto.IsSuccess)
                {
                    var errors = resultDto.Errors != null ? string.Join(", ", resultDto.Errors) : "Неизвестная ошибка API";
                    throw new HttpRequestException($"API ошибка: {errors}");
                }

                return resultDto.Data 
                        ?? throw new InvalidOperationException("Пустой блок Data в ответе сервера.");
            }
        }
        catch (JsonException)
        {
            // Если это даже не JSON — проваливаемся ниже к обработке ошибок или просто упадем на следующем шаге
        }

        // Если это просто JSON (массив фильмов, объект и т.д.) без обертки ResultDto
        try
        {
            var directData = JsonSerializer.Deserialize<T>(rawContent, options);
            return directData ?? throw new InvalidOperationException("Не удалось десериализовать прямой ответ сервера.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Ошибка десериализации данных типа {typeof(T).Name}. Детали: {ex.Message}");
        }
    }
}