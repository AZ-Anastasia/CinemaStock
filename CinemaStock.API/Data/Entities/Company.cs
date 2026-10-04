namespace CinemaStock.API.Data.Entities;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<CinemaContent> CinemaContents { get; set; } = [];
    public List<Game> DevelopedGames { get; set; } = [];
    public List<Game> PublishedGames { get; set; } = [];
}