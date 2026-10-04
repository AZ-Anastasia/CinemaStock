namespace CinemaStock.API.Data.Entities.Base;

public class Genre
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<CinemaContent> CinemaContents { get; set; } = [];
    public List<Game> Games { get; set; } = [];
}