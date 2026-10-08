using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.Auth;

/// <summary>
/// Development only: creates the admin account from the "Seed" settings,
/// so there is something to log in with. Does nothing if it already exists.
/// </summary>
public static class DevSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var normalized = email.Trim().ToLowerInvariant();
            if (await db.UserAccount.AnyAsync(u => u.Email.ToLower() == normalized))
                return;

            var user = new UserAccount { Email = normalized, PasswordHash = "", userRole = UserRole.Admin };
            user.PasswordHash = new PasswordHasher<UserAccount>().HashPassword(user, password);

            db.UserAccount.Add(user);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded admin account {Email}", normalized);
        }
        catch (Exception ex)
        {
            // Usually means the migrations have not run yet. The API still starts.
            logger.LogWarning(ex, "Could not seed the admin account");
        }
    }
}