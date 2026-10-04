using CinemaStock.Shared;
using Microsoft.AspNetCore.Components;

namespace CinemaStock.Client.Extensions;

public static class ImageExtensions
{
    [Inject] public static BackendSettings Settings { get; set; } = new();

    public static string GetPosterUrl(string? picturePath)
    {
        var baseUri = Settings.ApiBaseUrl;

        return string.IsNullOrEmpty(picturePath)
            ? Path.Combine(baseUri, ConstStrings.DefaultFilmImagePath)
            : Path.Combine(baseUri, picturePath);
    }
}