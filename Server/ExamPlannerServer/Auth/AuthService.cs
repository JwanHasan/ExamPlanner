using System.Security.Cryptography;
using System.Text;

namespace ExamPlannerServer.Auth;

/// <summary>
/// Checks the login against the account in appsettings ("Seed" section)
/// and returns a signed JWT when it matches.
/// When the UserAccount table exists, only LoginAsync needs to change.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly TokenService _tokens;

    public AuthService(IConfiguration config, TokenService tokens)
    {
        _config = config;
        _tokens = tokens;
    }

    public Task<string?> LoginAsync(string email, string password)
    {
        var validEmail = _config["Seed:AdminEmail"] ?? "";
        var validPassword = _config["Seed:AdminPassword"] ?? "";

        var emailOk = string.Equals(email.Trim(), validEmail, StringComparison.OrdinalIgnoreCase);
        var passwordOk = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(validPassword));

        if (!emailOk || !passwordOk || validEmail.Length == 0)
            return Task.FromResult<string?>(null);

        var token = _tokens.CreateToken(
            id: "1",
            email: validEmail,
            name: validEmail.Split('@')[0],
            role: "admin");

        return Task.FromResult<string?>(token);
    }
}