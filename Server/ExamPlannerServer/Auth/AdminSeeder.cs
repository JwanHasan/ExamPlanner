using DatabaseConnection.model;
using DatabaseConnection.repo;
using Microsoft.AspNetCore.Identity;

namespace ExamPlannerServer.Auth;

/// <summary>
/// Creates the first planner account from the "Seed" settings, so there is someone who can log in.
/// Runs at startup, does nothing if the account already exists, and never changes an existing password.
/// The values live in appsettings.Development.json; in production set Seed__AdminEmail and
/// Seed__AdminPassword as environment variables once.
///
/// The database only has the roles Teacher and Student, so the account is stored as Teacher.
/// It is "Admin" in the API because its email is listed in Auth:AdminEmails.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var email = config["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = config["Seed:AdminPassword"];
        var name = config["Seed:AdminName"] ?? "Exam planner";

        var admins = services.GetRequiredService<AdminSettings>();
        if (admins.AdminEmails.Count == 0)
            logger.LogWarning("Auth:AdminEmails is empty, so nobody can use the planner-only endpoints such as the import.");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return;

        if (!admins.IsAdmin(email))
            logger.LogWarning("{Email} is not listed in Auth:AdminEmails, so it can log in but cannot import.", email);

        try
        {
            using var scope = services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<IUserRepo>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            var exists = (await users.GetAllUsersAsync())
                .Any(u => string.Equals(u.Email.Trim(), email, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return;

            await users.AddUserAsync(email, name, hasher.HashPassword(new User(), password), Role.Teacher);
            logger.LogInformation("Created the planner account {Email}", email);
        }
        catch (Exception ex)
        {
            // Usually means the database has not been migrated yet. The API still starts.
            logger.LogWarning(ex, "Could not create the planner account {Email}. Has the database been migrated?", email);
        }
    }
}
