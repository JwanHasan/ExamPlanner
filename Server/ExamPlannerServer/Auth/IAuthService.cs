using ExamPlannerServer.Dto;

namespace ExamPlannerServer.Auth;

public interface IAuthService
{
    /// <summary>Checks email and password. Returns null when they do not match an account.</summary>
    Task<LoginResult?> LoginAsync(string email, string password);

    /// <summary>The account behind a token, or null if it no longer exists.</summary>
    Task<AuthUserDto?> GetUserAsync(int userId);
}

/// <summary>A successful login: the token and the user for the response body.</summary>
public record LoginResult(string Token, DateTime ExpiresAt, AuthUserDto User);
