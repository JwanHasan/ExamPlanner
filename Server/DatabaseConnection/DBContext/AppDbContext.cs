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
    
    public DbSet<UserAccount> UserAccount{get;set;}//*
    public DbSet<Class> Class {get;set;} //
    public DbSet<ExamClass> ExamClass{get;set;}//
    public DbSet<Course> Course {get;set;}//
    public DbSet<Enrollment> Enrollment {get;set;}//
    public DbSet<Exam>Exam {get;set;}//
    public DbSet<ExamDay> ExamDay{get;set;}//*
    public DbSet<ExamHandIn>ExamHandIn {get;set;}//
    public DbSet<ExamLecturer>ExamLecturer {get;set;}//
    public DbSet<ExamSession>ExamSession {get;set;}//
    public DbSet<ExamSessionRoom> ExamSessionRoom{get;set;}//*
    public DbSet<Lecturer>Lecturer {get;set;}//
    public DbSet<LecturerConstraint>LecturerConstraint {get;set;}//
    public DbSet<Room> Room{get;set;}//*
    public DbSet<Schedule> Schedule {get;set;}//*
    public DbSet<ScheduleReview> ScheduleReview{get;set;}//*
    public DbSet<Student>Student {get;set;}//
    public DbSet<StudentExamAssignment> StudentExamAssignment {get;set;}//*

    // setting up relation between tables 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>().HasKey(e => new {e.Id});

        modelBuilder.Entity<Course>()
        .HasMany(c=> c.Classes)
        .WithOne(o=> o.Course)
        .HasForeignKey(o=>o.CourseId);

        modelBuilder.Entity<Schedule>().HasKey(s=> new {s.Id});
        
        modelBuilder.Entity<ExamDay>().HasKey(e=> new {e.Id });
        modelBuilder.Entity<Schedule>()
        .HasMany(e=>e.ExamDays).WithOne(s=> s.Schedule).HasForeignKey(si=>si.ScheduleId);

        modelBuilder.Entity<ExamSession>().HasKey(e=> new {e.Id});
        modelBuilder.Entity<ExamSession>().HasOne(e=>e.Schedule).WithMany(e=>e.ExamSessions)
        .HasForeignKey(ex=>ex.ScheduleId);

        modelBuilder.Entity<Exam>().HasKey(e=> new {e.Id});
        modelBuilder.Entity<Exam>().HasMany(e=> e.ExamSessions).WithOne(e=> e.Exam).HasForeignKey(e=>e.ExamId);

        modelBuilder.Entity<ExamSessionRoom>().HasOne(e=>e.ExamSession)
        .WithMany(e=> e.ExamSessionRooms).HasForeignKey(e=> e.ExamSessionId);

        modelBuilder.Entity<Room>().HasKey(e=> new {e.Id});
        modelBuilder.Entity<Room>().HasMany(e=>e.ExamSessionRooms)
        .WithOne(e=> e.Room).HasForeignKey(e=> e.RoomId);

        modelBuilder.Entity<ScheduleReview>().HasKey(e=> new{e.LecturerId,e.ScheduleId});
        modelBuilder.Entity<ScheduleReview>().HasOne(e=>e.Schedule)
        .WithMany(e=>e.ScheduleReviews).HasForeignKey(e=>e.ScheduleId);


        modelBuilder.Entity<Lecturer>().HasKey(l=> new {l.Id});
        modelBuilder.Entity<Lecturer>().HasMany(e=>e.ScheduleReviews)
        .WithOne(l=>l.Lecturer).HasForeignKey(e=>e.LecturerId);

        modelBuilder.Entity<ExamLecturer>().HasKey(e=> new{e.ExamId,e.LecturerId});
        modelBuilder.Entity<ExamLecturer>().HasOne(e=> e.Lecturer)
        .WithMany(e=>e.ExamLecturers).HasForeignKey(e=>e.LecturerId);


        modelBuilder.Entity<LecturerConstraint>().HasKey(e=>new{e.Id});
        modelBuilder.Entity<LecturerConstraint>().HasOne(e=> e.Lecturer)
        .WithMany(e=> e.LecturerConstraints).HasForeignKey(e=>e.LecturerId);
        
        modelBuilder.Entity<Lecturer>().HasOne(e=>e.UserAccount)
        .WithOne(e=> e.LecturerAccount).HasForeignKey<Lecturer>(e=> e.UserAccountId);

        modelBuilder.Entity<ExamLecturer>().HasOne(e=>e.Exam)
        .WithMany(e=>e.ExamLecturers).HasForeignKey(e=>e.ExamId);

        modelBuilder.Entity<ExamHandIn>().HasKey(e=> new{e.Id});
        modelBuilder.Entity<ExamHandIn>().HasOne(e=>e.Exam)
        .WithMany(e=>e.ExamHandIns).HasForeignKey(e=>e.ExamId);

        modelBuilder.Entity<ExamClass>().HasKey(e=> new{e.ExamId,e.ClassId});

        modelBuilder.Entity<ExamClass>().HasOne(e=>e.Exam).WithMany(e=>e.ExamClasses)
        .HasForeignKey(e=>e.ExamId);
        modelBuilder.Entity<ExamClass>().HasOne(e=>e.Class).WithMany(e=>e.ExamClasses)
        .HasForeignKey(e=>e.ClassId);

        modelBuilder.Entity<StudentExamAssignment>().HasKey(e=> new {e.StudentId,e.ExamId});
        modelBuilder.Entity<StudentExamAssignment>().HasOne(e=> e.Exam)
        .WithMany(e=>e.StudentExamAssignments).HasForeignKey(e=>e.ExamId);
        modelBuilder.Entity<StudentExamAssignment>().HasOne(e=> e.Student)
        .WithMany(e=>e.ExamAssignments).HasForeignKey(e=>e.StudentId);

        modelBuilder.Entity<Enrollment>().HasKey(e=> new{ e.ClassId,e.StudentId});
        modelBuilder.Entity<Enrollment>().HasOne(e=> e.Student)
        .WithMany(e=>e.Enrollments).HasForeignKey(e=>e.StudentId);
        modelBuilder.Entity<Enrollment>().HasOne(e=> e.Class)
        .WithMany(e=>e.Enrollments).HasForeignKey(e=>e.ClassId);

        modelBuilder.Entity<Student>().HasKey(e=> new{e.Id});
        modelBuilder.Entity<Student>().HasOne(e=>e.UserAccount)
        .WithOne(e=>e.StudentAccount).HasForeignKey<Student>(e=>e.UserAccountId);

        modelBuilder.Entity<ExamSessionRoom>().HasKey(e=> new {e.ExamSessionId,e.RoomId});
    }
}