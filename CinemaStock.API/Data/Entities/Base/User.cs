using Microsoft.AspNetCore.Identity;

namespace CinemaStock.API.Data.Entities.Base;

public class User : IdentityUser<int>
{
    public List<UserMediaProgress> MediaProgress { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}