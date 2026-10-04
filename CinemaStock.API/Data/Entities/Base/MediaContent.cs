using CinemaStock.Shared.DTOs.Enums;

namespace CinemaStock.API.Data.Entities.Base;

/// <summary>
/// Базовый класс для фильмов/сериалов
/// </summary>
public abstract class MediaContent
{
    public Guid Id { get; set; }

    public string? LocalTitle { get; set; }
    public required string OriginalTitle { get; set; }
    public string? Picture { get; set; }
    public ReleaseType? Type { get; set; }
    public decimal Rating { get; set; }
    public string? Description { get; set; }
    public string? SourceInfoFrom { get; set; }

    public DateOnly? ReleaseDate { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}