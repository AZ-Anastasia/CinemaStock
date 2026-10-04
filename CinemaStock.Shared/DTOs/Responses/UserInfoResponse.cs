namespace CinemaStock.Shared.DTOs.Responses;

public class UserInfoResponse
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public List<string> Roles { get; set; } = new();
}