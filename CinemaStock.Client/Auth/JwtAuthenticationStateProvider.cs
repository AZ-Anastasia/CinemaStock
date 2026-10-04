using System.Security.Claims;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace CinemaStock.Client.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAccountService _accountService;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private const string TokenKey = "authToken";

    public JwtAuthenticationStateProvider(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userInfo = await _accountService.GetCurrentUserAsync();

            if (userInfo != null)
            {
                var identity = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, userInfo.Email),
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id)
                ], "CookieAuth");

                if (userInfo.Roles != null)
                {
                    foreach (var role in userInfo.Roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            else
                return new AuthenticationState(_anonymous);
        }
        catch (Exception)
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void MarkUserAsauthenticated(string email)
    {
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, email)
        ], "CookieAuth");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Account.UserLogout);
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            await _accountService.LogoutAsync();
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}