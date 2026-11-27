using DatabaseConnection.model;

namespace ExamPlannerServer.Dto;

/// <summary>
/// AuthUser from API_CONTRACT.md 24.1. Role is the API role: "Admin" for the exam planners
/// (listed in appsettings), otherwise the database role. The user's name is in the token itself.
/// </summary>
public record AuthUserDto(int Id, string Email, string Role)
{
    public static AuthUserDto From(User user, string role) => new(user.Id, user.Email, role);
}

/// <summary>Body of a successful login (API_CONTRACT.md 5.1).</summary>
public record LoginResponseDto(string AccessToken, DateTime ExpiresAt, AuthUserDto User);
