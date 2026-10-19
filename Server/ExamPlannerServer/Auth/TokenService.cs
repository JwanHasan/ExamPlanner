using DatabaseConnection.model;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ExamPlannerServer.Auth;

/// <summary>Creates the signed JWT that login returns as accessToken.</summary>
public sealed class TokenService
{
    private readonly JwtSettings _settings;

    public TokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    /// <param name="user">The account that logged in.</param>
    /// <param name="role">The API role (see <see cref="AdminSettings.RoleOf"/>): "Admin", "Teacher" or "Student".</param>
    public IssuedToken CreateToken(User user, string role)
    {
        var expiresAt = DateTime.UtcNow.AddHours(_settings.ExpiresHours);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(_settings.SigningKey(), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [AuthClaims.UserId] = user.Id.ToString(),
                [AuthClaims.Email] = user.Email,
                [AuthClaims.Name] = user.Name,
                [AuthClaims.Role] = role
            }
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}

/// <summary>A signed token and the moment it stops being valid (UTC).</summary>
public record IssuedToken(string Token, DateTime ExpiresAt);

public static class AuthClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";
}
