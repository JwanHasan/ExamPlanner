using DatabaseConnection.model;
using ExamPlannerServer.Planning;

namespace TesProject;

public sealed class PlanningRulesTests
{
    private static readonly DateOnly Monday =
        new(2026, 11, 23);

    private static EventData Event(
        int id,
        int[]? students = null,
        string[]? holds = null,
        int[]? teachers = null,
        ExamType type = ExamType.Oral,
        string code = "ADS1",
        string prefix = "IT"
    )
    {
        return new EventData(
            id,
            Guid.NewGuid(),
            $"event-{id}",
            code,
            $"course-{id}",
            prefix,
            type,
            1,
            1,
            students ?? [],
            holds ?? [],
            teachers ?? [],
            null,
            null,
            null,
            null,
            [],
            []
        );
    }

    private static SessionData Session(
        int id,
        int eventId,
        int campus = 1,
        int? room = 1,
        TimeOnly? start = null,
        TimeOnly? end = null,
        int[]? students = null,
        int[]? teachers = null
    )
    {
        return new SessionData(
            id,
            eventId,
            campus,
            room,
            Monday,
            start,
            end,
            "",
            false,
            "",
            students ?? [],
            teachers ?? []
        );
    }

    private static PlanningSnapshot Snapshot(
        EventData[] events,
        SessionData[] sessions
    )
    {
        return new PlanningSnapshot(
            events,
            sessions,

            [
                new RoomData(1, 1, 1),
                new RoomData(2, 2, 50)
            ],

            new()
            {
                [Monday] = true
            },
            [],
            [],
            [],
            []
        );
    }

    [Fact]
    public void AllTenRulesAreReportedWithEvidence()
    {
        var events = new[]
        {
            Event(1, [1, 2], ["H1"], [7]) with
            {
                RequiredDays = 2,
                AvoidWeekdays = [DayOfWeek.Monday]
            },
            Event(2, [1, 2], ["H1"], [7]),
            Event(3, [], ["H1"], [7])
        };

        var sessions = new[]
        {
            Session(1, 1, teachers: [7]),
            Session(2, 2, teachers: [7]),
            Session(3, 3, 2, 2, teachers: [7])
        };

        var snapshot = Snapshot(events, sessions) with
        {
            Days = new()
            {
                [Monday] = false
            },

            Constraints =
            [
                new ConstraintData(7, Monday, true)
            ]
        };

        var result =
            new ConflictService().Check(snapshot);

        Assert.Equal(
            ConflictService.Defaults.Keys.Order(),

            result.Conflicts
                .Select(x => x.RuleKey)
                .Distinct()
                .Order()
        );

        var student = Assert.Single(
            result.Conflicts,
            x => x.RuleKey == "student-double"
        );

        Assert.Equal(
            new[] { 1, 2 },
            student.AffectedStudentIds
        );

        Assert.Equal(
            new[] { 1, 2 },
            student.SessionIds
        );

        Assert.Equal(
            "High",
            student.Severity
        );

        Assert.All(
            result.Conflicts.Where(
                x => x.RuleKey == "hold-double"
            ),

            x => Assert.True(x.Possible)
        );
    }

    [Fact]
    public void AdjacentTimesDoNotOverlapButCrossCampusStillWarns()
    {
        var a = Session(
            1,
            1,
            start: new(9, 0),
            end: new(10, 0),
            teachers: [7]
        );

        var b = Session(
            2,
            2,
            2,
            2,
            new(10, 0),
            new(11, 0),
            teachers: [7]
        );

        var snapshot = Snapshot(
            [
                Event(1, [1]),
                Event(2, [1])
            ],

            [a, b]
        );

        var result =
            new ConflictService().Check(snapshot);

        Assert.False(
            ConflictService.Overlap(a, b)
        );

        Assert.DoesNotContain(
            result.Conflicts,

            x => x.RuleKey == "student-double"
        );

        Assert.Contains(
            result.Conflicts,

            x => x.RuleKey == "teacher-cross-campus"
        );

        Assert.True(
            ConflictService.Overlap(
                a,

                b with
                {
                    StartTime = null,
                    EndTime = null
                }
            )
        );
    }

    [Fact]
    public void ExplicitGroupsUseTheirOwnStudentsAndNotEntireCohort()
    {
        var snapshot = Snapshot(
            [
                Event(1, [1, 2]),
                Event(2, [1, 2])
            ],

            [
                Session(1, 1, students: [1]),

                Session(
                    2,
                    2,
                    room: null,
                    students: [2]
                )
            ]
        );

        var result =
            new ConflictService().Check(snapshot);

        Assert.DoesNotContain(
            result.Conflicts,

            x => x.RuleKey == "student-double"
        );
    }

    [Fact]
    public void RuleOverridesAndAcknowledgementsAreApplied()
    {
        var engine = new ConflictService();

        var snapshot = Snapshot(
            [
                Event(1, [1, 2]),
                Event(2, [1, 2])
            ],

            [
                Session(1, 1),
                Session(2, 2)
            ]
        );

        var conflict = Assert.Single(
            engine.Check(snapshot).Conflicts,
            x => x.RuleKey == "student-double"
        );

        snapshot = snapshot with
        {
            Acknowledged = [conflict.Fingerprint],

            Rules =
            [
                new RuleData(
                    "room-double",
                    false,
                    null
                ),

                new RuleData(
                    "student-double",
                    true,
                    "Low"
                )
            ]
        };

        var result = engine.Check(snapshot);

        Assert.DoesNotContain(
            result.Conflicts,

            x => x.RuleKey == "room-double"
        );

        var current = Assert.Single(
            result.Conflicts,
            x => x.RuleKey == "student-double"
        );

        Assert.Equal(
            "Low",
            current.Severity
        );

        Assert.True(
            current.Acknowledged
        );

        snapshot = snapshot with
        {
            Sessions =
            [
                snapshot.Sessions[0] with
                {
                    Date = Monday.AddDays(1)
                },

                snapshot.Sessions[1] with
                {
                    Date = Monday.AddDays(1)
                }
            ]
        };

        Assert.False(
            Assert.Single(
                engine.Check(snapshot).Conflicts,
                x => x.RuleKey == "student-double"
            ).Acknowledged
        );
    }

    [Fact]
    public void PriorityUsesNineTiersAndStableTieBreakers()
    {
        var events = new[]
        {
            Event(9),

            Event(8) with
            {
                LargeClass = true
            },

            Event(7) with
            {
                GraduatingStudents = true
            },

            Event(6) with
            {
                SmallSemester = true
            },

            Event(5) with
            {
                CoTaught = true
            },

            Event(
                4,
                type: ExamType.Sep,
                prefix: "XA"
            ),

            Event(
                3,
                code: "BPR2",
                prefix: "SW"
            ),

            Event(
                2,
                code: "BPR1",
                prefix: "XH"
            ),

            Event(
                1,
                type: ExamType.Written
            ),

            Event(
                10,
                [1, 2],
                type: ExamType.Written
            ),

            Event(
                11,
                [1],
                type: ExamType.Written
            ),

            Event(
                12,
                [1, 2],
                type: ExamType.Written
            )
        };

        var result = new PriorityService().Rank(
            Snapshot(events, [])
        );

        Assert.Equal(
            new[]
            {
                10, 12, 11, 1,
                2, 3, 4, 5,
                6, 7, 8, 9
            },

            result.Select(x => x.EventId)
        );

        Assert.Equal(
            Enumerable.Range(1, 9),

            result
                .Select(x => x.PriorityTier)
                .Distinct()
        );

        Assert.Equal(
            new[] { 11, 12 },

            result[0].RelatedEventIds
        );

        Assert.NotEmpty(
            result.Single(x => x.EventId == 9)
                .DataWarnings
        );
    }
}