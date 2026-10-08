namespace DatabaseConnection.DBContext;

using Microsoft.EntityFrameworkCore;

using DatabaseConnection.model;
using System.Reflection.Emit;
using System.Reflection;

public class AppDbContext : DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { 
    }
    
    public DbSet<Class> Class {get;set;} //
    public DbSet<Course> Course {get;set;}//
    public DbSet<Enrollment> Enrollment {get;set;}//
    public DbSet<Student> Student {get;set;}//
    public DbSet<User> User {get;set;}//

    

    // setting up relation between tables 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>().HasKey(s=> new {s.Id});
        modelBuilder.Entity<Class>().HasKey(c=> new {c.Id});
        modelBuilder.Entity<Enrollment>().HasKey(e=> new{e.StudentId,e.CourseId});
        modelBuilder.Entity<Student>().HasKey(s=> new{s.Id});
        modelBuilder.Entity<User>().HasKey(s=> new{s.Id});

        
        modelBuilder.Entity<Course>()
        .HasMany(c=> c.Classes)
        .WithOne(o=> o.Course)
        .HasForeignKey(o=>o.CourseId);

        modelBuilder.Entity<Enrollment>().HasOne(c => c.Course)
        .WithMany(e=> e.Enrollments).HasForeignKey(e=> e.CourseId);


        
        
        
    }

    
}