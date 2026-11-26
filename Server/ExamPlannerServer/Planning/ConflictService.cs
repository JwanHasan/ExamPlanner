using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ExamPlannerServer.Planning;

public sealed class ConflictService(
    ClashMapCache? cache = null
)
{
    private readonly ClashMapCache maps =
        cache ?? new ClashMapCache();

    public static readonly IReadOnlyDictionary<string, string>
        Defaults =
            new Dictionary<string, string>(
                StringComparer.Ordinal
            )
            {
                ["student-double"] = "High",
                ["hold-double"] = "Medium",
                ["teacher-cross-campus"] = "High",
                ["teacher-overlap"] = "Medium",
                ["teacher-unavailable"] = "High",
                ["room-double"] = "High",
                ["room-capacity"] = "Medium",
                ["day-unusable"] = "Medium",
                ["days-missing"] = "Info",
                ["preference"] = "Low"
            };

    public PlanningResult Check(PlanningSnapshot p)
    {
        var events = p.Events.ToDictionary(x => x.Id);
        var rooms = p.Rooms.ToDictionary(x => x.Id);
        var rules = p.Rules.ToDictionary(x => x.Key);
        var acknowledged = p.Acknowledged.ToHashSet();
        var map = maps.Get(p.Events);
        var result = new List<Conflict>();
        var warnings = new HashSet<string>();

        void Add(
            string key,
            SessionData[] sessions,
            int[] eventIds,
            string resource,
            string resourceId,
            string message,
            int[]? students = null,
            bool possible = false,
            string? severity = null,
            object? extra = null
        )
        {
            rules.TryGetValue(key, out var setting);

            if (setting is { Enabled: false })
            {
                return;
            }

            var ordered = sessions
                .OrderBy(s => s.Id)
                .ToArray();

            var ids = (students ?? [])
                .Distinct()
                .Order()
                .ToArray();

            var eids = eventIds
                .Distinct()
                .Order()
                .ToArray();

            var raw = JsonSerializer.Serialize(new
            {
                key,
                eids,
                resource,
                resourceId,

                sessions = ordered.Select(s => new
                {
                    s.Id,
                    s.EventId,
                    s.Date,
                    s.StartTime,
                    s.EndTime,
                    s.CampusId,
                    s.RoomId,
                    s.GroupLabel
                }),

                ids,
                possible,
                extra
            });

            var fingerprint = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(raw)
                )
            );

            result.Add(
                new Conflict(
                    key,
                    setting?.Severity
                        ?? severity
                        ?? Defaults[key],

                    ordered.Select(s => s.Id).ToArray(),

                    eids,

                    ordered.FirstOrDefault()?.Date,

                    ordered
                        .Select(s => s.CampusId)
                        .Distinct()
                        .Order()
                        .ToArray(),

                    resource,
                    resourceId,
                    ids,
                    message,
                    possible,
                    fingerprint,
                    acknowledged.Contains(fingerprint)
                )
            );
        }

        int[] Students(SessionData session)
        {
            return session.StudentIds.Length > 0
                ? session.StudentIds
                : events[session.EventId].StudentIds;
        }

        // Event-level validation.
        foreach (var e in p.Events)
        {
            if (e.StudentIds.Length == 0)
            {
                warnings.Add(
                    $"{e.Nickname}: no student registrations; " +
                    "student checks may be incomplete."
                );
            }

            if (e.StudentIds.Length == 0 &&
                e.Holds.Length == 0)
            {
                warnings.Add(
                    $"{e.Nickname}: no Hold data; fallback unavailable."
                );
            }

            if (e.RequiredDays <= 0)
            {
                warnings.Add(
                    $"{e.Nickname}: required exam days not specified."
                );
            }

            if (e.TeacherIds.Length == 0)
            {
                warnings.Add(
                    $"{e.Nickname}: no teacher data."
                );
            }

            var sessions = p.Sessions
                .Where(s => s.EventId == e.Id)
                .ToArray();

            var scheduledDays = sessions
                .Select(s => s.Date)
                .Distinct()
                .Count();

            if (e.RequiredDays > scheduledDays)
            {
                Add(
                    "days-missing",
                    sessions,
                    [e.Id],
                    "event",
                    e.Id.ToString(),
                    $"{e.Nickname} requires {e.RequiredDays} distinct exam days.",
                    extra: e.RequiredDays
                );
            }
        }

        // Individual session validation.
        foreach (var session in p.Sessions)
        {
            var e = events[session.EventId];

            if (!p.Days.TryGetValue(
                    session.Date,
                    out var usable
                ) ||
                !usable ||
                session.Date.DayOfWeek is
                    DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                Add(
                    "day-unusable",
                    [session],
                    [e.Id],
                    "day",
                    session.Date.ToString("yyyy-MM-dd"),
                    "Date is outside the usable exam days."
                );
            }

            if (session.TeacherIds.Length == 0)
            {
                warnings.Add(
                    $"Session {session.Id}: no assigned teachers; " +
                    "session checks may be incomplete."
                );
            }

            var constraints = p.Constraints
                .Where(c =>
                    c.Date == session.Date &&
                    session.TeacherIds.Contains(c.TeacherId)
                )
                .GroupBy(c => c.TeacherId);

            foreach (var constraint in constraints)
            {
                var hard = constraint.Any(x => x.Hard);

                Add(
                    "teacher-unavailable",
                    [session],
                    [e.Id],
                    "teacher",
                    constraint.Key.ToString(),
                    "Assigned teacher is unavailable.",
                    severity: hard ? "High" : "Low",
                    extra: hard
                );
            }

            if (session.RoomId is int roomId &&
                rooms.TryGetValue(roomId, out var room))
            {
                var students = Students(session);

                if (students.Length > room.Capacity)
                {
                    Add(
                        "room-capacity",
                        [session],
                        [e.Id],
                        "room",
                        roomId.ToString(),

                        $"{students.Length} students exceed " +
                        $"room capacity {room.Capacity}.",

                        students,

                        session.StudentIds.Length == 0 &&
                        (
                            e.RequiredDays > 1 ||
                            !string.IsNullOrEmpty(session.GroupLabel)
                        ),

                        extra: room.Capacity
                    );
                }
            }
            else if (!p.VirtualCampusIds.Contains(
                         session.CampusId
                     ))
            {
                warnings.Add(
                    $"Session {session.Id}: no room assigned."
                );
            }

            if (e.AvoidWeekdays.Contains(
                    session.Date.DayOfWeek
                ) ||
                e.AvoidDates.Contains(session.Date))
            {
                Add(
                    "preference",
                    [session],
                    [e.Id],
                    "event",
                    e.Id.ToString(),
                    "A recorded date preference is not respected."
                );
            }
        }

        // Comparing sessions occurring on the same date.
        foreach (var day in p.Sessions.GroupBy(s => s.Date))
        {
            var sessions = day
                .OrderBy(x => x.Id)
                .ToArray();

            for (var i = 0; i < sessions.Length; i++)
            {
                for (var j = i + 1; j < sessions.Length; j++)
                {
                    var x = sessions[i];
                    var y = sessions[j];

                    var ex = events[x.EventId];
                    var ey = events[y.EventId];

                    int[] eventIds =
                        [x.EventId, y.EventId];

                    SessionData[] pair = [x, y];

                    var overlap = Overlap(x, y);

                    if (overlap)
                    {
                        var shared =
                            x.StudentIds.Length == 0 &&
                            y.StudentIds.Length == 0 &&
                            x.EventId != y.EventId

                                ? map.Students(
                                    x.EventId,
                                    y.EventId
                                )

                                : Students(x)
                                    .Intersect(Students(y))
                                    .Order()
                                    .ToArray();

                        if (shared.Length > 0)
                        {
                            var possible =
                                (
                                    x.StudentIds.Length == 0 &&
                                    (
                                        ex.RequiredDays > 1 ||
                                        !string.IsNullOrEmpty(
                                            x.GroupLabel
                                        )
                                    )
                                )
                                ||
                                (
                                    y.StudentIds.Length == 0 &&
                                    (
                                        ey.RequiredDays > 1 ||
                                        !string.IsNullOrEmpty(
                                            y.GroupLabel
                                        )
                                    )
                                );

                            Add(
                                "student-double",
                                pair,
                                eventIds,
                                "student",
                                "pair",
                                $"{shared.Length} students have overlapping exams.",
                                shared,
                                possible
                            );
                        }

                        if (x.RoomId is int rid &&
                            y.RoomId == rid)
                        {
                            Add(
                                "room-double",
                                pair,
                                eventIds,
                                "room",
                                rid.ToString(),
                                "Room is double booked."
                            );
                        }
                    }

                    // Hold fallback when registrations are missing.
                    if (ex.StudentIds.Length == 0 ||
                        ey.StudentIds.Length == 0)
                    {
                        var holds = ex.Holds.Intersect(
                            ey.Holds,
                            StringComparer.OrdinalIgnoreCase
                        );

                        foreach (var hold in holds)
                        {
                            Add(
                                "hold-double",
                                pair,
                                eventIds,
                                "hold",
                                hold.ToUpperInvariant(),
                                "Shared Hold has exams on the same date.",
                                possible: true
                            );
                        }
                    }

                    // Teacher clashes.
                    foreach (var teacher in x.TeacherIds
                                 .Intersect(y.TeacherIds)
                                 .Order())
                    {
                        if (x.CampusId != y.CampusId)
                        {
                            Add(
                                "teacher-cross-campus",
                                pair,
                                eventIds,
                                "teacher",
                                teacher.ToString(),
                                "Teacher has exams at different campuses on the same date."
                            );
                        }
                        else if (overlap)
                        {
                            Add(
                                "teacher-overlap",
                                pair,
                                eventIds,
                                "teacher",
                                teacher.ToString(),
                                "Teacher has overlapping sessions at the same campus."
                            );
                        }
                    }
                }
            }
        }

        return new PlanningResult(
            p.Sessions,

            result
                .OrderBy(c => c.Date)
                .ThenBy(c => c.RuleKey)
                .ThenBy(c => c.Fingerprint)
                .ToArray(),

            warnings
                .Order()
                .ToArray()
        );
    }

    public static bool Overlap(
        SessionData a,
        SessionData b
    )
    {
        return a.Date == b.Date &&
               (
                   a.StartTime is null ||
                   a.EndTime is null ||
                   b.StartTime is null ||
                   b.EndTime is null ||
                   (
                       a.StartTime.Value < b.EndTime.Value &&
                       b.StartTime.Value < a.EndTime.Value
                   )
               );
    }
}