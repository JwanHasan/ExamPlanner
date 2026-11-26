using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.DBContext;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options
    ) : base(options)
    {
    }
    public DbSet<AssessmentEvent> AssessmentEvent { get; set; }
    public DbSet<AssessmentHandIn> AssessmentHandIn { get; set; }
    public DbSet<Class> Class { get; set; }
    public DbSet<Course> Course { get; set; }
    public DbSet<Enrollment> Enrollment { get; set; }
    public DbSet<ExamDay> ExamDay { get; set; }
    public DbSet<ExamSession> ExamSession { get; set; }
    public DbSet<PlanningEducationElement> PlanningEducationElement { get; set; }
    public DbSet<Room> Room { get; set; }
    public DbSet<Schedule> Schedule { get; set; }
    public DbSet<ScheduleReview> ScheduleReview { get; set; }
    public DbSet<Student> Student { get; set; }
    public DbSet<StudentExamAssignment> StudentExamAssignment { get; set; }
    public DbSet<Teacher> Teacher { get; set; }
    public DbSet<TeacherAssignment> TeacherAssignment { get; set; }
    public DbSet<TeacherConstraint> TeacherConstraint { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<CourseTeacher> CourseTeacher { get; set; }
    public DbSet<ConflictAcknowledgement> ConflictAcknowledgements { get; set; }
    public DbSet<RuleSetting> RuleSettings { get; set; }
    public DbSet<Campus> Campuses { get; set; }
    public DbSet<PlanningEventFacts> PlanningEventFacts { get; set; }
    public DbSet<ScheduleEditAudit> ScheduleEditAudits { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        modelBuilder.Entity<ExamSession>()
            .Property(x => x.RowVersion)
            .IsConcurrencyToken();

        modelBuilder.Entity<RuleSetting>()
            .HasIndex(x => x.RuleKey)
            .IsUnique();

        modelBuilder.Entity<ConflictAcknowledgement>()
            .HasIndex(x => new
            {
                x.ScheduleId,
                x.FingerPrint
            })
            .IsUnique();

        modelBuilder.Entity<PlanningEventFacts>()
            .HasKey(x => x.AssessmentEventId);

        modelBuilder.Entity<PlanningEventFacts>()
            .HasOne(x => x.AssessmentEvent)
            .WithOne()
            .HasForeignKey<PlanningEventFacts>(
                x => x.AssessmentEventId
            );

        modelBuilder.Entity<ScheduleEditAudit>()
            .HasIndex(x => x.ScheduleId);

        modelBuilder.Entity<ScheduleEditAudit>()
            .HasOne<Schedule>()
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ScheduleEditAudit>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Student>()
            .Property(x => x.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Enrollment>()
            .HasKey(x => new
            {
                x.StudentId,
                x.PlanningElementId
            });

        modelBuilder.Entity<StudentExamAssignment>()
            .HasKey(x => new
            {
                x.StudentId,
                x.ExamSessionId
            });

        modelBuilder.Entity<TeacherAssignment>()
            .HasKey(x => new
            {
                x.ExamSessionId,
                x.TeacherId
            });

        modelBuilder.Entity<ScheduleReview>()
            .HasKey(x => new
            {
                x.ScheduleId,
                x.TeacherId
            });

        modelBuilder.Entity<CourseTeacher>()
            .HasKey(x => new
            {
                x.CourseId,
                x.TeacherId
            });

        modelBuilder.Entity<Course>()
            .HasMany(x => x.Classes)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        modelBuilder.Entity<Course>()
            .HasMany(x => x.CourseTeachers)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        modelBuilder.Entity<Course>()
            .HasMany(x => x.PlanningEducationElements)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.PlanningEducationElement)
            .WithMany(x => x.Enrollment)
            .HasForeignKey(x => x.PlanningElementId);

        modelBuilder.Entity<Student>()
            .HasMany(x => x.Enrollments)
            .WithOne(x => x.Student)
            .HasForeignKey(x => x.StudentId);

        modelBuilder.Entity<Student>()
            .HasMany(x => x.StudentExamAssignment)
            .WithOne(x => x.Student)
            .HasForeignKey(x => x.StudentId);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.User)
            .WithOne(x => x.Student)
            .HasForeignKey<Student>(x => x.UserId);

        modelBuilder.Entity<Teacher>()
            .HasOne(x => x.User)
            .WithOne(x => x.Teacher)
            .HasForeignKey<Teacher>(x => x.UserId);

        modelBuilder.Entity<Teacher>()
            .HasMany(x => x.TeacherConstraints)
            .WithOne(x => x.Teacher)
            .HasForeignKey(x => x.TeacherId);

        modelBuilder.Entity<Teacher>()
            .HasMany(x => x.CourseTeachers)
            .WithOne(x => x.Teacher)
            .HasForeignKey(x => x.TeacherId);

        modelBuilder.Entity<Teacher>()
            .HasMany(x => x.TeacherAssignments)
            .WithOne(x => x.Teacher)
            .HasForeignKey(x => x.TeacherId);

        modelBuilder.Entity<Teacher>()
            .HasMany(x => x.ScheduleReviewss)
            .WithOne(x => x.Teacher)
            .HasForeignKey(x => x.TeacherId);

        modelBuilder.Entity<Schedule>()
            .HasMany(x => x.ExamDays)
            .WithOne(x => x.Schedule)
            .HasForeignKey(x => x.ScheduleId);

        modelBuilder.Entity<Schedule>()
            .HasMany(x => x.ScheduleReviews)
            .WithOne(x => x.Schedule)
            .HasForeignKey(x => x.ScheduleId);

        modelBuilder.Entity<ExamSession>()
            .HasMany(x => x.TeacherAssignments)
            .WithOne(x => x.ExamSession)
            .HasForeignKey(x => x.ExamSessionId);

        modelBuilder.Entity<ExamSession>()
            .HasMany(x => x.StudentExamAssignments)
            .WithOne(x => x.ExamSession)
            .HasForeignKey(x => x.ExamSessionId);

        modelBuilder.Entity<ExamSession>()
            .HasOne(x => x.AssessmentEvent)
            .WithMany(x => x.ExamSessions)
            .HasForeignKey(x => x.AsseessmenEventId);

        modelBuilder.Entity<ExamSession>()
            .HasOne(x => x.Schedule)
            .WithMany(x => x.ExamSessions)
            .HasForeignKey(x => x.ScheduleId);

        modelBuilder.Entity<ExamSession>()
            .HasOne(x => x.Room)
            .WithMany(x => x.ExamSessions)
            .HasForeignKey(x => x.RoomId);

        modelBuilder.Entity<ExamSession>()
            .HasOne(x => x.Campus)
            .WithMany(x => x.ExamSessions)
            .HasForeignKey(x => x.CampusId);

        modelBuilder.Entity<AssessmentEvent>()
            .HasMany(x => x.AssessmentHandIns)
            .WithOne(x => x.AssessmentEvent)
            .HasForeignKey(x => x.AssessmentEventId);

        modelBuilder.Entity<AssessmentEvent>()
            .HasOne(x => x.PlanningEducationElement)
            .WithOne(x => x.AssessmentEvent)
            .HasForeignKey<AssessmentEvent>(
                x => x.PlanningElementId
            );

        modelBuilder.Entity<ConflictAcknowledgement>()
            .HasOne(x => x.User)
            .WithMany(x => x.ConflictAcknowledgement)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<ConflictAcknowledgement>()
            .HasOne(x => x.Schedule)
            .WithMany(x => x.ConflictAcknowledgements)
            .HasForeignKey(x => x.ScheduleId);

        modelBuilder.Entity<ConflictAcknowledgement>()
            .HasOne(x => x.RuleSetting)
            .WithMany(x => x.ConflictAcknowledgements)
            .HasForeignKey(x => x.RuleKeyId);

        modelBuilder.Entity<Room>()
            .HasOne(x => x.Campus)
            .WithMany(x => x.Rooms)
            .HasForeignKey(x => x.CampusId);

        modelBuilder.Entity<PlanningEducationElement>()
            .HasOne(x => x.Campus)
            .WithMany(x => x.PlanningEducationElements)
            .HasForeignKey(x => x.CampusId);
    }
}