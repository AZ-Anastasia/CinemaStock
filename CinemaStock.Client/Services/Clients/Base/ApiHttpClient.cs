using System.Net.Http.Json;
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

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("Пустой ответ от сервера.");
    }
}