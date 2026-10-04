namespace CinemaStock.API.Data.Entities.Base;

/// <summary>
/// Прогресс пользователя
/// </summary>
public class UserMediaProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public MediaContent? MediaContent { get; set; }

    public double? OwnRate { get; set; }
    public string MyComment { get; set; } = string.Empty;

    public List<CustomList> CustomList { get; set; } = [];
}