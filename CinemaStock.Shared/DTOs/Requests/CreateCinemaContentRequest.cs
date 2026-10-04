using System.ComponentModel.DataAnnotations;
using CinemaStock.Shared.DTOs.Enums;

namespace CinemaStock.Shared.DTOs.Requests;


public class CreateCinemaContentRequest
{
    public string? LocalTitle { get; set; }
    [Required]
    public string? OriginalTitle { get; set; }
    public string? Picture { get; set; }
    public ReleaseType? Type { get; set; } = ReleaseType.Unknown;
    public DateOnly? ReleaseDate { get; set; }
    public decimal? Rating { get; set; } = 0;
    public string? Description { get; set; }
    public string? ReleaseStudio { get; set; }
    public string? OwnComment { get; set; }
    public string? SourceInfoFrom { get; set; }
    public List<Guid> GenresIds { get; set; } = new();
    public List<Guid> TagsIds { get; set; } = new();
}