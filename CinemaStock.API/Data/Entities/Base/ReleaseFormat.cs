using CinemaStock.Shared.DTOs.Enums;

namespace CinemaStock.API.Data.Entities.Base;

public class ReleaseFormat
{
    public ReleaseType Id { get; set; }
    public required string FormatName { get; set; }

    public List<MediaContent> MediaContents { get; set; } = [];
}