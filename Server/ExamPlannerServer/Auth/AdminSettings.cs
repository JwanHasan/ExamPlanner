using DatabaseConnection.model;

namespace ExamPlannerServer.Auth;

/// <summary>
/// The "Auth" section of appsettings: which accounts are exam planners (role "Admin" in the API).
///
/// The database only has the roles Teacher and Student, so "Admin" is not stored there.
/// An account is Admin when its email is listed in AdminEmails; its User row keeps a database
/// role (Teacher for the planner account that AdminSeeder creates).
/// </summary>
public sealed class AdminSettings
{
    public const string Section = "Auth";

    /// <summary>The API role of the exam planners (API_CONTRACT.md, sections 5.1 and 6.1).</summary>
    public const string AdminRole = "Admin";

    public List<string> AdminEmails { get; set; } = new();

    public bool IsAdmin(string email) =>
        AdminEmails.Any(admin => string.Equals(admin.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>The role the API uses for this user: "Admin" for planners, otherwise the database role.</summary>
    public string RoleOf(User user) => IsAdmin(user.Email) ? AdminRole : user.Role.ToString();
}
