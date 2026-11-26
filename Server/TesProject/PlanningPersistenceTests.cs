using DatabaseConnection.model;
using ExamPlannerServer.Planning;
using Microsoft.EntityFrameworkCore;

namespace TesProject;

public sealed class PlanningPersistenceTests
{
    [Fact]
    public async Task PreviewDoesNotSave_CommitAllowsWarnings_AcknowledgeAndStaleEditWork()
    {
        await using var test =
            await TestDatabase.CreateAsync(DbKind.Sqlite);

        await using var db = test.NewContext();

        var user = new User
        {
            Name = "Planner",
            Email = "planner@test.invalid",
            Role = Role.Admin
        };

        var campus = new Campus
        {
            Name = "Horsens"
        };

        var course = new Course
        {
            Name = "Example",
            Prefix = "IT",
            SueCode = "ADS1"
        };

        var day = new DateOnly(2026, 11, 23);
        var schedule = new Schedule
        {
            Version = "test",

            ExamDays =
            [
                new ExamDay
                {
                    Date = day.ToDateTime(
                        TimeOnly.MinValue,
                        DateTimeKind.Utc
                    ),

                    Usable = false
                }
            ]
        };

        var element = new PlanningEducationElement
        {
            Id = Guid.NewGuid(),
            Campus = campus,
            Course = course,
            Name = "Example",
            Nickname = "IT-ADS1",
            SourceModifiedAt = DateTime.UtcNow
        };

        var e = new AssessmentEvent
        {
            PlanningEducationElement = element,
            RequiredExamDays = 1,
            ExamType = ExamType.Written
        };

        db.AddRange(user, schedule, e);

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var service = new PlanningService(
            db,
            new ConflictService(),
            new PriorityService()
        );

        var request = new EditRequest(
            [e.Id],

            [
                new SessionChange(
                    null,
                    null,
                    false,
                    e.Id,
                    campus.Id,
                    null,
                    day,
                    null,
                    null,
                    "",
                    false
                )
            ]
        );
        
        var preview = await service.EditAsync(
            schedule.Id,
            request,
            user.Id,
            false,
            default
        );

        Assert.Contains(
            preview.Conflicts,

            x => x.RuleKey == "day-unusable"
        );

        Assert.Equal(
            0,
            await db.ExamSession.CountAsync()
        );

        Assert.Equal(
            0,
            await db.ScheduleEditAudits.CountAsync()
        );

        var saved = await service.EditAsync(
            schedule.Id,
            request,
            user.Id,
            true,
            default
        );

        var row = Assert.Single(saved.Sessions);

        Assert.True(row.Id > 0);

        Assert.NotEmpty(row.RowVersion);

        Assert.Equal(
            1,
            await db.ExamSession.CountAsync()
        );

        Assert.Equal(
            1,
            await db.ScheduleEditAudits.CountAsync()
        );

        var conflict = Assert.Single(
            saved.Conflicts,
            x => x.RuleKey == "day-unusable"
        );

        var acknowledged =
            await service.AcknowledgeAsync(
                schedule.Id,

                new AcknowledgeRequest(
                    conflict.Fingerprint,
                    [e.Id],
                    "Accepted"
                ),

                user.Id,
                default
            );

        Assert.True(
            Assert.Single(
                acknowledged.Conflicts,
                x => x.RuleKey == "day-unusable"
            ).Acknowledged
        );

        Assert.Equal(
            1,
            await db.ConflictAcknowledgements.CountAsync()
        );

        var stale = new EditRequest(
            [e.Id],

            [
                new SessionChange(
                    row.Id,
                    "old",
                    false,
                    e.Id,
                    campus.Id,
                    null,
                    day,
                    null,
                    null,
                    "",
                    false
                )
            ]
        );

        var error =
            await Assert.ThrowsAsync<PlanningException>(
                () => service.EditAsync(
                    schedule.Id,
                    stale,
                    user.Id,
                    true,
                    default
                )
            );

        Assert.Equal(
            409,
            error.Status
        );

        Assert.Equal(
            1,
            await db.ScheduleEditAudits.CountAsync()
        );
    }
}