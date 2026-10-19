using CinemaStock.Client.Services.Clients.Base;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Resources.UserErrors;
using CinemaStock.Shared.Routes;

namespace CinemaStock.Client.Services.Clients;

public class AccountClient : ApiHttpClient, IAccountService
{
    public AccountClient(HttpClient http) : base(http) { }

    public async Task<UserInfoResponse?> GetCurrentUserAsync()
    {
        try
        {
            return await GetAsync<UserInfoResponse>(ApiRoutes.Account.CurrentUser);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ResultDto<UserBaseInfoResponse>> LoginAsync(string email, string password)
    {
        try
        {
            var loginDto = new LoginRequest { Email = email, Password = password };
            var loggedUser = await PostAsync<LoginRequest, UserBaseInfoResponse>(ApiRoutes.Account.Login, loginDto);

            return new ResultDto<UserBaseInfoResponse>(true, loggedUser, null);
        }
        catch (HttpRequestException ex)
        {
            return new ResultDto<UserBaseInfoResponse>(false, new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            return new ResultDto<UserBaseInfoResponse>(false, new List<string> { $"{UserErrors.ServerConnectionFailed}. {ex.Message}" });
        }
    }

    public async Task<ResultDto<UserBaseInfoResponse>> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var dto = new RegisterRequest { Username = username, Email = email, Password = password };
            var registeredUser = await PostAsync<RegisterRequest,UserBaseInfoResponse>(ApiRoutes.Account.Register, dto);

            return new ResultDto<UserBaseInfoResponse>(true, registeredUser, null);
        }
        catch (HttpRequestException ex)
        {
            return new ResultDto<UserBaseInfoResponse>(false, new List<string> { ex.Message });
        }
        catch (Exception ex)
        {
            return new ResultDto<UserBaseInfoResponse>(false, new List<string> { $"{UserErrors.ServerConnectionFailed}. {ex.Message}" });
        }
    }

    public async Task LogoutAsync()
    {
        await PostAsync(ApiRoutes.Account.LogoutEndpoint);
    }
}