using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.Auth;

/// <summary>Checks email and password, and returns a token when they match.</summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly PasswordHasher<UserAccount> _hasher = new();

    public AuthService(AppDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var normalized = email.Trim().ToLowerInvariant();

        var user = await _db.UserAccount
            .Include(u => u.LecturerAccount)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);

        if (user is null)
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        var name = user.LecturerAccount?.Initials ?? user.Email.Split('@')[0];
        return _tokens.CreateToken(
            user.Id.ToString(),
            user.Email,
            name,
            user.userRole.ToString().ToLowerInvariant());
    }
}