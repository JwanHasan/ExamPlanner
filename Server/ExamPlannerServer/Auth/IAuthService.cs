using ExamPlannerServer.Dto;

namespace ExamPlannerServer.Auth;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(string email, string password);

    /// <summary>The account behind a token, or null if it no longer exists.</summary>
    Task<AuthUserDto?> GetUserAsync(int userId);
}

public record LoginResult(string Token, DateTime ExpiresAt, AuthUserDto User);
