using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using AdessoWorldLeague.Business.Interfaces;
using AdessoWorldLeague.Business.Resources;
using AdessoWorldLeague.Dto.Auth;

namespace AdessoWorldLeague.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IStringLocalizer<Messages> _localizer;

    public AuthController(IAuthService authService, IStringLocalizer<Messages> localizer)
    {
        _authService = authService;
        _localizer = localizer;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = _localizer["RegistrationSuccessful"].Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }
}
