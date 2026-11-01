using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMarket.Application.Features.Auth;

namespace SportMarket.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>ثبت‌نام کاربر جدید</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    /// <summary>ورود کاربر</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    /// <summary>اطلاعات کاربر جاری</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var result = await _sender.Send(new GetCurrentUserQuery());
        return Ok(result);
    }
}
