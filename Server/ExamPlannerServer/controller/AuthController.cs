using ExamPlannerServer.Auth;
using ExamPlannerServer.Contracts;
using ExamPlannerServer.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamPlannerServer.controller;

/// <summary>
/// Login with JWT (API_CONTRACT.md, section 5).
/// Login returns the token as accessToken. The website keeps it in a cookie and sends it back
/// on every request as "Authorization: Bearer &lt;token&gt;" (section 3.2).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // POST /api/v1/auth/login  (5.1)
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var email = request.Email?.Trim() ?? "";
        if (email.Length == 0 || !email.Contains('@') || string.IsNullOrEmpty(request.Password))
            return BadRequest(new ApiError(ErrorCodes.InvalidLoginRequest, "Enter your email address and password."));

        var result = await _auth.LoginAsync(email, request.Password);
        if (result is null)
            return Unauthorized(new ApiError(ErrorCodes.InvalidCredentials, "Email or password is incorrect."));

        return Ok(new LoginResponseDto(result.Token, result.ExpiresAt, result.User));
    }

    // GET /api/v1/auth/me  (5.2) -> who is logged in
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirst(AuthClaims.UserId)?.Value, out var userId))
            return Unauthorized(new ApiError(ErrorCodes.Unauthorized, "You need to log in."));

        var user = await _auth.GetUserAsync(userId);
        if (user is null)
            return Unauthorized(new ApiError(ErrorCodes.Unauthorized, "Your account no longer exists. Log in again."));

        return Ok(user);
    }
}
