namespace CinemaStock.API.Data.Entities.Base;

/// <summary>
/// Кастомный список для сортировки (создается пользователем)
/// </summary>
public class CustomList
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public List<UserMediaProgress> UserMediaProgresses { get; set; } = [];
}