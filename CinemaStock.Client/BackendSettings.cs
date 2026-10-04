namespace CinemaStock.Client;

public class BackendSettings
{
    private string _apiBaseUrl = string.Empty;

    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => _apiBaseUrl = value?.TrimEnd('/') ?? string.Empty;
    }
}