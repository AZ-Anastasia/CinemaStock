using System.Security.Claims;
using CinemaStock.API.Data.Entities.Base;
using CinemaStock.Shared.ApiEndpointControllers;
using CinemaStock.Shared.DTOs.Requests;
using CinemaStock.Shared.DTOs.Responses;
using CinemaStock.Shared.Resources.UserErrors;
using CinemaStock.Shared.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaStock.API.Controllers;

[ApiController]
[Route(ApiRoutes.Account.Base)]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Account.RegisterEndpoint)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var newUser = new User
        {
            UserName = model.Username,
            Email = model.Email,
        };

        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (result.Succeeded)
        {
            var userResponse = new UserBaseInfoResponse
            {
                Username = newUser.UserName,
                Email = newUser.Email
            };

            var successResult = new ResultDto<UserBaseInfoResponse>(true, userResponse, null);
            return Ok(successResult);
        }

        var errorMessages = result.Errors.Select(e => e.Description).ToList();
        var errorResult = new ResultDto<UserBaseInfoResponse>(false, null, errorMessages);
        return BadRequest(errorResult);
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Account.LoginEndpoint)]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
            return BadRequest(new ResultDto<UserInfoResponse>(false, null, new() { UserErrors.BadLoginOrPassword }));

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var customClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));

            await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, customClaims);

            var loginData = new UserInfoResponse
            {
                Id = user.Id.ToString(),
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            };

            var successResult = new ResultDto<UserInfoResponse>(true, loginData, null);
            return Ok(successResult);
        }

        var errors = new List<string>();

        if (result.IsLockedOut)
            errors.Add(UserErrors.AccountBlocked);
        else if (result.IsNotAllowed)
            errors.Add(UserErrors.LoginIsNotAllowed);
        else if (result.RequiresTwoFactor)
            errors.Add(UserErrors.TwoFactorAuthRequired);
        else
            errors.Add(UserErrors.BadLoginOrPassword);

        return BadRequest(new ResultDto<UserInfoResponse>(false, null, errors));
    }

    [Authorize]
    [HttpPost(ApiRoutes.Account.LogoutEndpoint)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [Authorize]
    [HttpGet(ApiRoutes.Account.CurrentUserEndpoint)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogWarning("Пользователь из контекста куки не найден в системе.");
                return Unauthorized(new ResultDto<UserInfoResponse>(false, null, new() { "Пользователь не найден." }));
            }

            IList<string> roles = new List<string>();

            try
            {
                roles = await _userManager.GetRolesAsync(user);
            }
            catch (Exception roleEx)
            {
                _logger.LogError(roleEx, "Ошибка при получении ролей пользователя {Email} из БД. Проверьте конфигурацию Identity.", user.Email);
            }
            var response = new UserInfoResponse
            {
                Id = user.Id.ToString(),
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList() ?? new List<string>()
            };

            return Ok(new ResultDto<UserInfoResponse>(true, response, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой в методе GetCurrentUser.");

            return StatusCode(500, new ResultDto<UserInfoResponse>(false, null, new() { UserErrors.ServerConnectionFailed }));
        }
    }
}
