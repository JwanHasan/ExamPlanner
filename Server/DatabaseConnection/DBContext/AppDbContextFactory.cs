namespace DatabaseConnection.DBContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DatabaseConnection.model;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string [] args)
    {
        var optionsBuilder= new DbContextOptionsBuilder<AppDbContext>();

        //setting up connection to database
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ExamPlanner;Username=postgres;Password=viaviavia"
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}