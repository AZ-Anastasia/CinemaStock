namespace CinemaStock.API.Data.Entities;

public class PlatformOS
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public List<Game> Games { get; set; } = [];
}