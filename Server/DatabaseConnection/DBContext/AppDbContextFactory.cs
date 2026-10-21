namespace DatabaseConnection.DBContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DatabaseConnection.model;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string [] args)
    {
        var optionsBuilder= new DbContextOptionsBuilder<AppDbContext>();
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var connectionString =
        $"Host={host};Port={port};Database={database};Username={username};Password={password}";

        //setting up connection to database
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}