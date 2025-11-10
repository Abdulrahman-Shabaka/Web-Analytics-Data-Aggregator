using AnalyticsAggregator.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsAggregator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var token = await _userService.RegisterAsync(request.Email, request.Password, request.Name);
            return Ok(new { Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _userService.LoginAsync(request.Email, request.Password);
        if (token == null)
        {
            return Unauthorized(new { Error = "Invalid email or password" });
        }

        return Ok(new { Token = token });
    }
}

public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);

