using CinemaStock.Shared.DTOs.Responses;

namespace CinemaStock.Shared.ApiEndpointControllers;

public interface IAccountService
{
    Task<ResultDto> RegisterAsync(string username, string email, string password);
    Task<ResultDto> LoginAsync(string email, string password);
    Task<UserInfoResponse?> GetCurrentUserAsync();
    Task LogoutAsync();
}