namespace DatabaseConnection.DBContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DatabaseConnection.model;
using DotNetEnv;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string [] args)
    {
         // Search the current directory and its parent directories for .env
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory != null &&
               !File.Exists(Path.Combine(directory.FullName, ".env")))
        {
            directory = directory.Parent;
        }

        // A .env file is optional: on a developer machine it supplies the settings, but in Docker there is
        // none (the settings are passed in as real environment variables), so no file is not an error.
        if (directory != null)
        {
            Env.Load(Path.Combine(directory.FullName, ".env"));
        }
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