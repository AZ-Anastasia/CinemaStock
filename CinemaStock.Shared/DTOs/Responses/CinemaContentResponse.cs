namespace CinemaStock.Shared.DTOs.Responses;

public record CinemaContentResponse(
    Guid Id,
    string? LocalTitle,
    string OriginalTitle,
    string? Picture,
    string Type,
    DateOnly? ReleaseDate,
    decimal Rating,
    string? Description,
    IdNameResponse? ReleaseStudio,
    List<string> Genres,
    List<string> Tags
);