using DatabaseConnection.model;
using DatabaseConnection.repo;
using ExamPlannerServer.Dto;
using Microsoft.AspNetCore.Identity;

namespace ExamPlannerServer.Auth;

/// <summary>
/// Login against the User table through the teammates' IUserRepo. Passwords are stored as salted
/// PBKDF2 hashes made by ASP.NET's PasswordHasher, never as plain text.
/// </summary>
public sealed class AuthService : IAuthService
{
    // Checked when the email is unknown, so a wrong email takes as long as a wrong password.
    // Otherwise the response time would reveal which emails have an account.
    private static readonly string TimingDummyHash =
        new PasswordHasher<User>().HashPassword(new User(), "timing-dummy-password");

    private readonly IUserRepo _users;
    private readonly IPasswordHasher<User> _hasher;
    private readonly TokenService _tokens;
    private readonly AdminSettings _admins;

    public AuthService(IUserRepo users, IPasswordHasher<User> hasher, TokenService tokens, AdminSettings admins)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _admins = admins;
    }

    public async Task<LoginResult?> LoginAsync(string email, string password)
    {
        var user = await FindByEmailAsync(email);

        if (user is null)
        {
            _hasher.VerifyHashedPassword(new User(), TimingDummyHash, password);
            return null;
        }

        var check = Verify(user, password);
        if (check == PasswordVerificationResult.Failed)
            return null;

        // The hashing settings got stronger since this password was saved: store a new hash.
        if (check == PasswordVerificationResult.SuccessRehashNeeded)
            await _users.UpdateHashPasswordById(user.Id, _hasher.HashPassword(user, password));

        var role = _admins.RoleOf(user);
        var token = _tokens.CreateToken(user, role);
        return new LoginResult(token.Token, token.ExpiresAt, AuthUserDto.From(user, role));
    }

    public async Task<AuthUserDto?> GetUserAsync(int userId)
    {
        var user = await _users.GetUserByIdAsync(userId);
        return user is null ? null : AuthUserDto.From(user, _admins.RoleOf(user));
    }

    // IUserRepo has no "get by email", so this looks through all users. That is fine for the
    // number of accounts here; a GetUserByEmailAsync in UserRepo would be the faster way.
    // Accounts without a password (for example imported students) can never log in.
    private async Task<User?> FindByEmailAsync(string email)
    {
        var wanted = email.Trim();
        return (await _users.GetAllUsersAsync())
            .Where(u => u.HashedPassword.Length > 0 &&
                        string.Equals(u.Email.Trim(), wanted, StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.Id)
            .FirstOrDefault();
    }

    private PasswordVerificationResult Verify(User user, string password)
    {
        try
        {
            return _hasher.VerifyHashedPassword(user, user.HashedPassword, password);
        }
        catch (FormatException)
        {
            // The column holds something that is not a PasswordHasher hash, for example a password
            // typed straight into the database. Treat it as a wrong password instead of crashing.
            return PasswordVerificationResult.Failed;
        }
    }
}
