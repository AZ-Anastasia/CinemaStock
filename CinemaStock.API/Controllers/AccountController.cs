using System.Security.Claims;
using CinemaStock.API.Data.Entities.Base;
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
            return Ok();
        }

        var errorMessages = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(errorMessages);
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Account.LoginEndpoint)]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
            return BadRequest(new[] { UserErrors.BadLoginOrPassword });

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var customClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));

            await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, customClaims);
            
            return Ok();
        }

        if (result.IsLockedOut)
            return BadRequest(new[] { UserErrors.AccountBlocked });

        if (result.IsNotAllowed)
            return BadRequest(new[] { UserErrors.LoginIsNotAllowed });

        if (result.RequiresTwoFactor)
            return BadRequest(new[] { UserErrors.TwoFactorAuthRequired });

        return BadRequest(new[] { UserErrors.BadLoginOrPassword });
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
                return Unauthorized();
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

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критический сбой в методе GetCurrentUser.");

            return StatusCode(500, UserErrors.ServerConnectionFailed);
        }
    }
}
