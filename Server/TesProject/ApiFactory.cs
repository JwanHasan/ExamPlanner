using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TesProject;

// Starts the real API (Program.cs, controllers, JWT, DI) inside the test process, with two changes:
//  - the database is an in-memory SQLite database instead of PostgreSQL
//  - the settings come from here, so the tests do not depend on anyone's appsettings or secrets
// Tests talk to it with a normal HttpClient, exactly like the website does.
//
// Any public type from the API project can be the type argument: it is only used to find the project.
public sealed class ApiFactory : WebApplicationFactory<ExcelImportParser>, IAsyncLifetime
{
    public const string PlannerEmail = "planner@test.dk";   // listed in Auth:AdminEmails, so the API role is "Admin"
    public const string TeacherEmail = "teacher@test.dk";   // an ordinary teacher
    public const string Password = "Test-Passw0rd!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not "Development", so the developer's appsettings.Development.json (with its seed account) is not used.
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Key", "test-only-signing-key-0123456789-abcdefghij");
        builder.UseSetting("Jwt:Issuer", "ExamPlannerTests");
        builder.UseSetting("Jwt:Audience", "ExamPlannerTests");
        builder.UseSetting("Jwt:ExpiresHours", "1");
        builder.UseSetting("Auth:AdminEmails:0", PlannerEmail);

        builder.ConfigureServices(services =>
        {
            // Program.cs registers AppDbContext for PostgreSQL. Remove everything that belongs to it
            // (the context, its options, and EF's option configuration) and register it again for SQLite.
            var registrations = services
                .Where(d => d.ServiceType == typeof(AppDbContext)
                            || d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                            || (d.ServiceType.IsGenericType && d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext))))
                .ToList();

            foreach (var registration in registrations)
                services.Remove(registration);

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        // Building the host (first use of Services) runs Program.cs. After that the tables are created
        // and two accounts are added: a planner (Admin in the API) and an ordinary teacher.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        foreach (var (email, name) in new[] { (PlannerEmail, "Test Planner"), (TeacherEmail, "Test Teacher") })
        {
            var user = new User { Email = email, Name = name, Role = Role.Teacher };
            user.HashedPassword = hasher.HashPassword(user, Password);
            db.User.Add(user);
        }

        await db.SaveChangesAsync();
    }

    // Lets a test look into the database, for example to check what an import saved.
    public async Task<T> QueryDatabaseAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
