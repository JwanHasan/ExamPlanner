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
        modelBuilder.Entity<Enrollment>().HasKey(e=> new{e.StudentId,e.CourseId});
        modelBuilder.Entity<StudentExamAssignment>().HasKey(s=> new{s.StudentId,s.ExamSessionId});
        modelBuilder.Entity<TeacherAssignment>().HasKey(s=> new{s.ExamSessionId,s.TeacherId});
        modelBuilder.Entity<ScheduleReview>().HasKey(s=> new{s.ScheduleId,s.TeacherId});




        modelBuilder.Entity<Course>()
        .HasMany(c=> c.Classes)
        .WithOne(o=> o.Course)
        .HasForeignKey(o=>o.CourseId);

        modelBuilder.Entity<Course>()
        .HasMany(o=> o.AssessmentEvents)
        .WithOne(c=>c.Course).HasForeignKey(o=>o.CourseId);

        modelBuilder.Entity<Enrollment>().HasOne(c => c.Course)
        .WithMany(e=> e.Enrollments).HasForeignKey(e=> e.CourseId);



        modelBuilder.Entity<Student>().HasMany(o=> o.Enrollments)
        .WithOne(s=> s.Student).HasForeignKey(s=> s.StudentId);

        modelBuilder.Entity<Student>().HasMany(x=> x.StudentExamAssignment)
        .WithOne(s=> s.Student).HasForeignKey(s=> s.StudentId);

        modelBuilder.Entity<Student>().HasOne(x=> x.User).WithOne(x=>x.Student)
        .HasForeignKey<Student>(x=> x.UserId);


        modelBuilder.Entity<Teacher>().HasOne(x=> x.User).WithOne(x=>x.Teacher)
        .HasForeignKey<Teacher>(x=> x.UserId);

        modelBuilder.Entity<Teacher>().HasMany(x=> x.TeacherConstraints)
        .WithOne(x=> x.Teacher).HasForeignKey(x=> x.TeacherId);

        modelBuilder.Entity<Teacher>().HasMany(x=> x.TeacherAssignments)
        .WithOne(x=> x.Teacher).HasForeignKey(x=> x.TeacherId);

        modelBuilder.Entity<Teacher>().HasMany(x=> x.ScheduleReviewss)
        .WithOne(x=> x.Teacher).HasForeignKey(x=> x.TeacherId);



        modelBuilder.Entity<Schedule>().HasMany(x=> x.ExamDays)
        .WithOne(x=> x.Schedule).HasForeignKey(x=> x.ScheduleId);

        modelBuilder.Entity<Schedule>().HasMany(x=> x.ScheduleReviews)
        .WithOne(x=> x.Schedule).HasForeignKey(x=> x.ScheduleId);

        
        
        modelBuilder.Entity<ExamSession>().HasMany(x=> x.TeacherAssignments)
        .WithOne(x=> x.ExamSession).HasForeignKey(x=> x.ExamSessionId);

        modelBuilder.Entity<ExamSession>().HasMany(x=> x.StudentExamAssignments)
        .WithOne(x=> x.ExamSession).HasForeignKey(x=> x.ExamSessionId);

        modelBuilder.Entity<ExamSession>().HasOne(x=> x.AssessmentEvent)
        .WithMany(x=> x.ExamSessions).HasForeignKey(x=> x.AsseessmentId);

        modelBuilder.Entity<ExamSession>().HasOne(x=> x.Schedule)
        .WithMany(x=> x.ExamSessions).HasForeignKey(x=> x.ScheduleId);

        modelBuilder.Entity<ExamSession>().HasOne(x=> x.Room)
        .WithMany(x=> x.ExamSessions).HasForeignKey(x=> x.RoomId);


        modelBuilder.Entity<AssessmentEvent>().HasMany(x=> x.AssessmentHandIns)
        .WithOne(x=> x.AssessmentEvent).HasForeignKey(x=> x.AssessmentEventId);

        modelBuilder.Entity<AssessmentEvent>().HasOne(x=> x.PlanningEducationElement)
        .WithOne(x=> x.AssessmentEvent).HasForeignKey<AssessmentEvent>(x=> x.PlanningElementId);

        modelBuilder.Entity<AssessmentEvent>().HasOne(x=> x.Course)
        .WithMany(x=> x.AssessmentEvents).HasForeignKey(x=> x.CourseId);

    }

    
}