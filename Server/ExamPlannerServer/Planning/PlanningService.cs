using System.Text.Json;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.Planning;

public sealed class PlanningException(
    string message,
    int status = 400
) : Exception(message)
{
    public int Status { get; } = status;
}

public sealed class PlanningService(
    AppDbContext db,
    ConflictService conflicts,
    PriorityService priority
)
{
    public async Task<PlanningSnapshot> LoadAsync(
        int scheduleId,
        int[] eventIds,
        CancellationToken ct
    )
    {
        if (eventIds is null)
        {
            throw new PlanningException(
                "eventIds is required."
            );
        }

        if (!await db.Schedule.AnyAsync(
                s => s.Id == scheduleId,
                ct
            ))
        {
            throw new PlanningException(
                "Schedule not found.",
                404
            );
        }

        var sessions = await LoadSessionsAsync(
            scheduleId,
            ct
        );
        var ids = eventIds
            .Concat(sessions.Select(x => x.EventId))
            .Distinct()
            .ToArray();
        var events = await LoadEventsAsync(ids, ct);
        var rooms = await db.Room
            .AsNoTracking()
            .Select(x => new RoomData(
                x.Id,
                x.CampusId,
                x.Capacity
            ))
            .ToArrayAsync(ct);
        var days = await db.ExamDay
            .AsNoTracking()
            .Where(x => x.ScheduleId == scheduleId)
            .ToListAsync(ct);
        var constraints = await db.TeacherConstraint
            .AsNoTracking()
            .ToListAsync(ct);
        var settings = await db.RuleSettings
            .AsNoTracking()
            .Where(x => x.RuleKey != null)
            .ToListAsync(ct);
        var acknowledgements =
            await db.ConflictAcknowledgements
                .AsNoTracking()
                .Where(x => x.ScheduleId == scheduleId)
                .Select(x => x.FingerPrint)
                .ToArrayAsync(ct);
        var virtualCampuses = await db.Campuses
            .AsNoTracking()
            .Where(x => x.IsVirtual)
            .Select(x => x.Id)
            .ToArrayAsync(ct);

        return new PlanningSnapshot(
            events,
            sessions,
            rooms,

            days
                .GroupBy(x => DateOnly.FromDateTime(x.Date))
                .ToDictionary(
                    g => g.Key,
                    g => g.All(x => x.Usable)
                ),

            constraints
                .Select(x => new ConstraintData(
                    x.TeacherId,
                    x.ConstraintDate,
                    x.ConstraintType == ConstraintType.MustToHave
                ))
                .ToArray(),

            settings
                .Select(x => new RuleData(
                    x.RuleKey!,
                    x.Enabled,

                    x.Severity == "Default"
                        ? null
                        : x.Severity
                ))
                .ToArray(),

            acknowledgements,
            virtualCampuses
        );
    }

    private async Task<SessionData[]> LoadSessionsAsync(
        int scheduleId,
        CancellationToken ct
    )
    {
        var rows = await db.ExamSession
            .AsNoTracking()
            .Where(x => x.ScheduleId == scheduleId)
            .Include(x => x.StudentExamAssignments)
            .Include(x => x.TeacherAssignments)
            .AsSplitQuery()
            .ToListAsync(ct);

        return rows
            .OrderBy(x => x.Id)
            .Select(ToSession)
            .ToArray();
    }

    private static SessionData ToSession(
        ExamSession x
    )
    {
        var students =
            (x.StudentExamAssignments ?? [])
                .OrderBy(a => a.StudentId)
                .Select(a => new StudentAllocation(
                    a.StudentId,
                    a.ExtraTime
                ))
                .ToArray();

        var teachers =
            (x.TeacherAssignments ?? [])
                .OrderBy(a => a.TeacherId)
                .Select(a => new TeacherAllocation(
                    a.TeacherId,
                    a.Role
                ))
                .ToArray();

        var wholeDay =
            x.StartTime == x.EndTime &&
            x.StartTime == TimeOnly.MinValue;

        return new SessionData(
            x.Id,
            x.AsseessmenEventId,
            x.CampusId,
            x.RoomId,
            x.Date,

            wholeDay ? null : x.StartTime,
            wholeDay ? null : x.EndTime,

            x.GroupLabel,
            x.LockDate,
            x.RowVersion,

            students.Select(a => a.StudentId).ToArray(),
            teachers.Select(a => a.TeacherId).ToArray(),

            students,
            teachers
        );
    }

    private async Task<EventData[]> LoadEventsAsync(
        int[] ids,
        CancellationToken ct
    )
    {
        var rows = await db.AssessmentEvent
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.PlanningEducationElement)
            .ThenInclude(x => x.Course)
            .Include(x => x.PlanningEducationElement)
            .ThenInclude(x => x.Enrollment)
            .Include(x => x.PlanningEducationElement)
            .ThenInclude(x => x.Course)
            .ThenInclude(x => x.CourseTeachers)
            .AsSplitQuery()
            .ToListAsync(ct);

        if (rows.Count != ids.Length)
        {
            throw new PlanningException(
                "One or more assessment event IDs do not exist.",
                404
            );
        }

        var facts = await db.PlanningEventFacts
            .AsNoTracking()
            .Where(x => ids.Contains(x.AssessmentEventId))
            .ToDictionaryAsync(
                x => x.AssessmentEventId,
                ct
            );

        return rows.Select(e =>
        {
            var p = e.PlanningEducationElement;
            var course = p.Course;

            facts.TryGetValue(e.Id, out var f);

            var enrollments = p.Enrollment ?? [];

            return new EventData(
                e.Id,
                p.Id,
                p.Nickname,
                course.SueCode,
                course.Name,
                course.Prefix,
                e.ExamType,
                e.RequiredExamDays,
                p.CampusId,

                enrollments
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Order()
                    .ToArray(),

                enrollments
                    .Select(x => x.Hold)
                    .Concat(Read<string>(f?.HoldsJson))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),

                (course.CourseTeachers ?? [])
                    .Select(x => x.TeacherId)
                    .Distinct()
                    .Order()
                    .ToArray(),

                f?.CoTaught,
                f?.SmallSemester,
                f?.GraduatingStudents,
                f?.LargeClass,

                Read<DayOfWeek>(f?.AvoidWeekdaysJson),
                Read<DateOnly>(f?.AvoidDatesJson)
            );
        }).ToArray();
    }

    private static T[] Read<T>(
        string? json
    )
    {
        return json is null
            ? []
            : JsonSerializer.Deserialize<T[]>(json) ?? [];
    }

    private static PlanningSnapshot WithSessionResources(
        PlanningSnapshot p
    )
    {
        return p with
        {
            Events = p.Events.Select(e => e with
            {
                TeacherIds = e.TeacherIds
                    .Concat(
                        p.Sessions
                            .Where(s => s.EventId == e.Id)
                            .SelectMany(s => s.TeacherIds)
                    )
                    .Distinct()
                    .Order()
                    .ToArray(),

                StudentIds = e.StudentIds
                    .Concat(
                        p.Sessions
                            .Where(s => s.EventId == e.Id)
                            .SelectMany(s => s.StudentIds)
                    )
                    .Distinct()
                    .Order()
                    .ToArray()
            }).ToArray()
        };
    }

    public async Task<PlanningResult> CheckAsync(
        int scheduleId,
        int[] ids,
        CancellationToken ct
    )
    {
        var snapshot = await LoadAsync(
            scheduleId,
            ids,
            ct
        );

        return conflicts.Check(
            WithSessionResources(snapshot)
        );
    }

    public async Task<PriorityItem[]> RankAsync(
        int scheduleId,
        int[] ids,
        CancellationToken ct
    )
    {
        if (ids is null || ids.Length == 0)
        {
            throw new PlanningException(
                "Select assessment event IDs for this planning period."
            );
        }

        var snapshot = WithSessionResources(
            await LoadAsync(
                scheduleId,
                ids,
                ct
            )
        );

        return priority
            .Rank(snapshot)
            .Where(x => ids.Contains(x.EventId))
            .ToArray();
    }

    public async Task<PlanningResult> EditAsync(
        int scheduleId,
        EditRequest request,
        int userId,
        bool commit,
        CancellationToken ct
    )
    {
        if (request.EventIds is null ||
            request.Changes is null ||
            request.Changes.Length == 0 ||
            request.Changes.Any(x => x is null))
        {
            throw new PlanningException(
                "Supply eventIds and at least one change."
            );
        }

        if (request.Changes
            .Where(x => x.Id.HasValue)
            .GroupBy(x => x.Id)
            .Any(g => g.Count() > 1))
        {
            throw new PlanningException(
                "A session may occur only once in an edit batch."
            );
        }

        await using var tx = commit
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        var ids = request.EventIds
            .Concat(
                request.Changes
                    .Where(x => !x.Delete)
                    .Select(x => x.EventId)
            )
            .Distinct()
            .ToArray();
        var snapshot = await LoadAsync(
            scheduleId,
            ids,
            ct
        );
        ids = snapshot.Events
            .Select(x => x.Id)
            .ToArray();
        var existing = snapshot.Sessions
            .ToDictionary(x => x.Id);
        var candidate =
            new Dictionary<int, SessionData>(existing);
        var campusIds =
            (await db.Campuses
                .AsNoTracking()
                .Select(x => x.Id)
                .ToArrayAsync(ct))
            .ToHashSet();
        var studentIds = request.Changes
            .SelectMany(x => x.Students ?? [])
            .Select(x => x.StudentId)
            .Distinct()
            .ToArray();
        var teacherIds = request.Changes
            .SelectMany(x => x.Teachers ?? [])
            .Select(x => x.TeacherId)
            .Distinct()
            .ToArray();

        if (await db.Student.CountAsync(
                x => studentIds.Contains(x.Id),
                ct
            ) != studentIds.Length ||

            await db.Teacher.CountAsync(
                x => teacherIds.Contains(x.Id),
                ct
            ) != teacherIds.Length)
        {
            throw new PlanningException(
                "Unknown student or teacher in assignment."
            );
        }
        var nextId = -1;
        var edits =
            new List<(
                SessionChange Change,
                SessionData? Data
            )>();

        foreach (var change in request.Changes)
        {
            SessionData? old = null;

            if (change.Id is int id)
            {
                if (!existing.TryGetValue(id, out old))
                {
                    throw new PlanningException(
                        "Session not found in this schedule.",
                        404
                    );
                }

                if (change.RowVersion is null ||
                    change.RowVersion != old.RowVersion)
                {
                    throw new PlanningException(
                        "The session changed. Reload it before editing.",
                        409
                    );
                }
            }

            if (change.Delete)
            {
                if (old is null)
                {
                    throw new PlanningException(
                        "Delete requires an existing session ID."
                    );
                }

                candidate.Remove(old.Id);

                edits.Add((change, null));

                continue;
            }

            if (!campusIds.Contains(change.CampusId) ||
                change.Date == default)
            {
                throw new PlanningException(
                    "A valid campus and date are required."
                );
            }

            if (change.RoomId is int roomId &&
                !snapshot.Rooms.Any(r =>
                    r.Id == roomId &&
                    r.CampusId == change.CampusId
                ))
            {
                throw new PlanningException(
                    "Room must exist and belong to the selected campus."
                );
            }

            if (change.StartTime.HasValue !=
                change.EndTime.HasValue ||

                (
                    change.StartTime.HasValue &&
                    change.StartTime.Value >= change.EndTime!.Value
                ))
            {
                throw new PlanningException(
                    "Both times must be supplied and start must precede end, " +
                    "or both must be null."
                );
            }

            var students =
                change.Students ?? old?.Students ?? [];
            var teachers =
                change.Teachers ?? old?.Teachers ?? [];

            if (students
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count() != students.Length ||

                teachers
                    .Select(x => x.TeacherId)
                    .Distinct()
                    .Count() != teachers.Length ||

                teachers.Any(x => !Enum.IsDefined(x.Role)))
            {
                throw new PlanningException(
                    "Duplicate or invalid assignment."
                );
            }

            if (old is not null &&
                old.EventId != change.EventId &&
                (
                    change.Students is null ||
                    change.Teachers is null
                ))
            {
                throw new PlanningException(
                    "Changing an event requires explicit student and teacher assignments."
                );
            }

            var row = new SessionData(
                old?.Id ?? nextId--,
                change.EventId,
                change.CampusId,
                change.RoomId,
                change.Date,
                change.StartTime,
                change.EndTime,
                change.GroupLabel?.Trim() ?? "",
                change.Locked,
                old?.RowVersion ?? "",

                students
                    .Select(x => x.StudentId)
                    .Order()
                    .ToArray(),

                teachers
                    .Select(x => x.TeacherId)
                    .Order()
                    .ToArray(),

                students,
                teachers
            );
            candidate[row.Id] = row;

            edits.Add((change, row));
        }

        if (!commit)
        {
            return conflicts.Check(
                WithSessionResources(
                    snapshot with
                    {
                        Sessions = candidate.Values
                            .OrderBy(x => x.Id)
                            .ToArray()
                    }
                )
            );
        }

        var tracked = await db.ExamSession
            .Where(x => x.ScheduleId == scheduleId)
            .Include(x => x.StudentExamAssignments)
            .Include(x => x.TeacherAssignments)
            .AsSplitQuery()
            .ToDictionaryAsync(x => x.Id, ct);

        foreach (var (change, data) in edits)
        {
            var row = change.Id is int id
                ? tracked.GetValueOrDefault(id)
                : new ExamSession
                {
                    ScheduleId = scheduleId
                };

            if (row is null ||
                (
                    change.Id.HasValue &&
                    row.RowVersion != change.RowVersion
                ))
            {
                throw new PlanningException(
                    "The session changed. Reload it before editing.",
                    409
                );
            }

            if (data is null)
            {
                db.ExamSession.Remove(row);

                continue;
            }

            if (!change.Id.HasValue)
            {
                // Prevents default navigation objects from being inserted as new database records.
                row.AssessmentEvent = null!;
                row.Schedule = null!;

                db.ExamSession.Add(row);
            }

            row.AsseessmenEventId = data.EventId;
            row.CampusId = data.CampusId;
            row.RoomId = data.RoomId;
            row.Date = data.Date;
            row.StartTime = data.StartTime;
            row.EndTime = data.EndTime;
            row.GroupLabel = data.GroupLabel;
            row.LockDate = data.Locked;
            row.RowVersion = Guid.NewGuid().ToString("N");
            row.StudentExamAssignments ??= new List<StudentExamAssignment>();
            row.TeacherAssignments ??= new List<TeacherAssignment>();

            var wantedStudents =
                (data.Students ?? [])
                    .ToDictionary(x => x.StudentId);
            var wantedTeachers =
                (data.Teachers ?? [])
                    .ToDictionary(x => x.TeacherId);

            foreach (var assignment in
                     row.StudentExamAssignments.ToArray())
            {
                if (wantedStudents.Remove(
                        assignment.StudentId,
                        out var wanted
                    ))
                {
                    assignment.ExtraTime =
                        wanted.ExtraTime;
                }
                else
                {
                    db.StudentExamAssignment.Remove(
                        assignment
                    );
                }
            }

            foreach (var assignment in wantedStudents.Values)
            {
                row.StudentExamAssignments.Add(
                    new StudentExamAssignment
                    {
                        StudentId = assignment.StudentId,
                        ExtraTime = assignment.ExtraTime
                    }
                );
            }

            foreach (var assignment in
                     row.TeacherAssignments.ToArray())
            {
                if (wantedTeachers.Remove(
                        assignment.TeacherId,
                        out var wanted
                    ))
                {
                    assignment.Role = wanted.Role;
                }
                else
                {
                    db.TeacherAssignment.Remove(
                        assignment
                    );
                }
            }

            foreach (var assignment in wantedTeachers.Values)
            {
                row.TeacherAssignments.Add(
                    new TeacherAssignment
                    {
                        TeacherId = assignment.TeacherId,
                        Role = assignment.Role
                    }
                );
            }
        }

        await db.SaveChangesAsync(ct);

        var after = await LoadAsync(
            scheduleId,
            ids,
            ct
        );

        db.ScheduleEditAudits.Add(
            new ScheduleEditAudit
            {
                ScheduleId = scheduleId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,

                BeforeJson =
                    JsonSerializer.Serialize(snapshot.Sessions),

                AfterJson =
                    JsonSerializer.Serialize(after.Sessions)
            }
        );

        await db.SaveChangesAsync(ct);

        var result = conflicts.Check(
            WithSessionResources(after)
        );

        // Conflicts are warnings, not reasons to reject an edit.
        await tx!.CommitAsync(ct);
        return result;
    }

    public async Task SetFactsAsync(
        int id,
        FactsRequest request,
        CancellationToken ct
    )
    {
        if (!await db.AssessmentEvent.AnyAsync(
                x => x.Id == id,
                ct
            ))
        {
            throw new PlanningException(
                "Event not found.",
                404
            );
        }

        if (request.Holds is null ||
            request.AvoidWeekdays is null ||
            request.AvoidDates is null ||
            request.AvoidWeekdays.Any(x => !Enum.IsDefined(x)))
        {
            throw new PlanningException(
                "Invalid planning facts."
            );
        }

        var facts = await db.PlanningEventFacts.FindAsync(
            new object[] { id },
            ct
        );

        if (facts is null)
        {
            facts = new PlanningEventFacts
            {
                AssessmentEventId = id
            };

            db.PlanningEventFacts.Add(facts);
        }

        facts.CoTaught = request.CoTaught;
        facts.SmallSemester = request.SmallSemester;
        facts.GraduatingStudents = request.GraduatingStudents;
        facts.LargeClass = request.LargeClass;

        facts.HoldsJson = JsonSerializer.Serialize(
            request.Holds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
        );

        facts.AvoidWeekdaysJson = JsonSerializer.Serialize(
            request.AvoidWeekdays.Distinct()
        );

        facts.AvoidDatesJson = JsonSerializer.Serialize(
            request.AvoidDates.Distinct()
        );

        await db.SaveChangesAsync(ct);
    }

    public async Task<RuleSetting> SetRuleAsync(
        string key,
        RuleChange request,
        CancellationToken ct
    )
    {
        if (!ConflictService.Defaults.ContainsKey(key) ||
            request.Severity is not
                ("High" or "Medium" or "Low" or "Info" or "Default"))
        {
            throw new PlanningException(
                "Unknown rule or severity (High, Medium, Low, Info, Default)."
            );
        }

        var row = await db.RuleSettings.SingleOrDefaultAsync(
            x => x.RuleKey == key,
            ct
        );

        if (row is null)
        {
            row = new RuleSetting
            {
                RuleKey = key,
                Severity = request.Severity
            };

            db.RuleSettings.Add(row);
        }
        row.Enabled = request.Enabled;
        row.Severity = request.Severity;

        await db.SaveChangesAsync(ct);

        return row;
    }

    public async Task<PlanningResult> AcknowledgeAsync(
        int scheduleId,
        AcknowledgeRequest request,
        int userId,
        CancellationToken ct
    )
    {
        await using var tx =
            await db.Database.BeginTransactionAsync(ct);

        var current = await CheckAsync(
            scheduleId,
            request.EventIds,
            ct
        );

        var conflict = current.Conflicts.SingleOrDefault(
            x => x.Fingerprint == request.Fingerprint
        );

        if (conflict is null)
        {
            throw new PlanningException(
                "Conflict is no longer current. Refresh the warnings.",
                409
            );
        }

        if (!conflict.Acknowledged)
        {
            var rule = await db.RuleSettings.SingleOrDefaultAsync(
                x => x.RuleKey == conflict.RuleKey,
                ct
            );

            if (rule is null)
            {
                rule = new RuleSetting
                {
                    RuleKey = conflict.RuleKey,
                    Enabled = true,
                    Severity = "Default"
                };

                db.RuleSettings.Add(rule);
            }

            db.ConflictAcknowledgements.Add(
                new ConflictAcknowledgement
                {
                    ScheduleId = scheduleId,
                    RuleSetting = rule,
                    UserId = userId,
                    FingerPrint = conflict.Fingerprint,

                    Reason =
                        request.Reason?.Trim() ?? "",

                    SnapshotJson =
                        JsonSerializer.Serialize(conflict),

                    CreatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);

        return await CheckAsync(
            scheduleId,
            request.EventIds,
            ct
        );
    }
}