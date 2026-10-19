using CinemaStock.Shared.DTOs.Responses;

namespace CinemaStock.Shared.ApiEndpointControllers;

public interface IAccountService
{
    Task<ResultDto<UserBaseInfoResponse>> RegisterAsync(string username, string email, string password);
    Task<ResultDto<UserBaseInfoResponse>> LoginAsync(string email, string password);
    Task<UserInfoResponse?> GetCurrentUserAsync();
    Task LogoutAsync();
}