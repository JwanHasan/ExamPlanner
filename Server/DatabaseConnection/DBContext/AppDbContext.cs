using Microsoft.EntityFrameworkCore;

using DatabaseConnection.model;
public class AppDbContext : DbContext
{
    
        
    public DbSet<Class> Class {get;set;}
    public DbSet<ExamClass> ExamClass{get;set;}
    public DbSet<Course> Course {get;set;}
    public DbSet<Enrollment> Enrollment {get;set;}
    public DbSet<Exam>Exam {get;set;}
    public DbSet<ExamHandIn>ExamHandIn {get;set;}
    public DbSet<ExamLecturer>ExamLecturer {get;set;}
    public DbSet<ExamSession>ExamSession {get;set;}
    public DbSet<Lecturer>Lecturer {get;set;}
    public DbSet<LecturerConstraint>LecturerConstraint {get;set;}
    public DbSet<Student>Student {get;set;}



    //setting up connection to database 

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ExamPlanner;Username=postgres;Password=viaviavia");
    }


    // setting up relation between tables 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Course>()
        
        .HasMany(c=> c.Classes)
        .WithOne(o=> o.Course)
        .HasForeignKey(o=>o.CourseId);

        modelBuilder.Entity<Enrollment>()
        .HasKey(e => new { e.StudentId, e.ClassId });

        modelBuilder.Entity<ExamClass>()
        .HasKey(e => new { e.ClassId, e.ExamId });

        modelBuilder.Entity<ExamClass>()
        .HasOne(e=>e.Exam)
        .WithMany(ec=>ec.ExamClasses)
        .HasForeignKey(c=>c.ExamId)
        .IsRequired();

        modelBuilder.Entity<ExamClass>()
        .HasOne(e=>e.Class)
        .WithMany(ec=>ec.ExamClasses)
        .HasForeignKey(c=>c.ClassId)
        .IsRequired();

        modelBuilder.Entity<Enrollment>()
        .HasOne(c=>c.Class)
        .WithMany(en=>en.Enrollments)
        .HasForeignKey(c=>c.ClassId)
        .IsRequired();

        modelBuilder.Entity<Enrollment>()
        .HasOne(c=>c.Student)
        .WithMany(en=>en.Enrollments)
        .HasForeignKey(c=>c.StudentId)
        .IsRequired();

        modelBuilder.Entity<ExamSession>()
        .HasOne(e=>e.Exam)
        .WithMany(ec=>ec.ExamSessions)
        .HasForeignKey(c=>c.ExamId)
        .IsRequired();

        modelBuilder.Entity<ExamHandIn>()
        .HasOne(e=>e.Exam)
        .WithMany(ec=>ec.ExamHandIns)
        .HasForeignKey(c=>c.ExamId)
        .IsRequired();

modelBuilder.Entity<ExamLecturer>()
        .HasKey(e => new { e.LecturerId, e.ExamId });

        modelBuilder.Entity<ExamLecturer>()
        .HasOne(e=>e.Exam)
        .WithMany(ec=>ec.ExamLecturers)
        .HasForeignKey(c=>c.ExamId)
        .IsRequired();

        modelBuilder.Entity<ExamLecturer>()
        .HasOne(e=>e.Lecturer)
        .WithMany(ec=>ec.ExamLecturers)
        .HasForeignKey(c=>c.LecturerId)
        .IsRequired();

         modelBuilder.Entity<LecturerConstraint>()
        .HasOne(e=>e.Lecturer)
        .WithMany(ec=>ec.LecturerConstraints)
        .HasForeignKey(c=>c.LecturerId)
        .IsRequired();


        
    }

    
}