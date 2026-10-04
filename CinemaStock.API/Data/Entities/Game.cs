using CinemaStock.API.Data.Entities.Base;

namespace CinemaStock.API.Data.Entities;

public class Game
{
    public Guid Id { get; set; }
    public Guid? DeveloperId { get; set; }
    public Company? Developer { get; set; }

    public Guid? PublisherId { get; set; }
    public Company? Publisher { get; set; }

    public List<PlatformOS> Platform { get; set; } = [];

    public List<Genre> GameGenres { get; set; } = [];
    public List<Tag> GameTags { get; set; } = [];
}