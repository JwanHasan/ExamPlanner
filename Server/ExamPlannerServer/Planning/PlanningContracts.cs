using DatabaseConnection.model;

namespace ExamPlannerServer.Planning;

public sealed record StudentAllocation(
    int StudentId,
    bool ExtraTime
);

public sealed record TeacherAllocation(
    int TeacherId,
    TeacherRole Role
);

public sealed record SessionData(
    int Id,
    int EventId,
    int CampusId,
    int? RoomId,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string GroupLabel,
    bool Locked,
    string RowVersion,
    int[] StudentIds,
    int[] TeacherIds,
    StudentAllocation[]? Students = null,
    TeacherAllocation[]? Teachers = null
);

public sealed record SessionChange(
    int? Id,
    string? RowVersion,
    bool Delete,
    int EventId,
    int CampusId,
    int? RoomId,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? GroupLabel,
    bool Locked,
    StudentAllocation[]? Students = null,
    TeacherAllocation[]? Teachers = null
);

public sealed record EditRequest(
    int[] EventIds,
    SessionChange[] Changes
);

public sealed record ExamDayInput(
    DateOnly Date,
    ExamDayType Type,
    bool Usable
);

public sealed record ScheduleCreateRequest(
    string Version,
    ExamDayInput[] Days
);

public sealed record EventCreateRequest(
    Guid PlanningElementId,
    ExamType ExamType,
    int RequiredExamDays,
    string? Note
);

public sealed record PriorityRequest(
    int[] EventIds
);

public sealed record RuleChange(
    bool Enabled,
    string Severity
);

public sealed record AcknowledgeRequest(
    string Fingerprint,
    int[] EventIds,
    string? Reason
);

public sealed record FactsRequest(
    bool? CoTaught,
    bool? SmallSemester,
    bool? GraduatingStudents,
    bool? LargeClass,
    string[] Holds,
    DayOfWeek[] AvoidWeekdays,
    DateOnly[] AvoidDates
);

public sealed record Conflict(
    string RuleKey,
    string Severity,
    int[] SessionIds,
    int[] EventIds,
    DateOnly? Date,
    int[] CampusIds,
    string ResourceType,
    string ResourceId,
    int[] AffectedStudentIds,
    string Message,
    bool Possible,
    string Fingerprint,
    bool Acknowledged
)
{
    public int AffectedCount => AffectedStudentIds.Length;
}

public sealed record PlanningResult(
    SessionData[] Sessions,
    Conflict[] Conflicts,
    string[] DataWarnings
);

public sealed record PriorityItem(
    int EventId,
    Guid PlanningElementId,
    string Nickname,
    string CourseCode,
    string CourseName,
    int PriorityTier,
    string Reason,
    int ClashCount,
    int StudentCount,
    int[] RelatedEventIds,
    string[] DataWarnings
);

public sealed record EventData(
    int Id,
    Guid PlanningElementId,
    string Nickname,
    string Code,
    string Name,
    string Prefix,
    ExamType Type,
    int RequiredDays,
    int CampusId,
    int[] StudentIds,
    string[] Holds,
    int[] TeacherIds,
    bool? CoTaught,
    bool? SmallSemester,
    bool? GraduatingStudents,
    bool? LargeClass,
    DayOfWeek[] AvoidWeekdays,
    DateOnly[] AvoidDates
);

public sealed record RoomData(
    int Id,
    int CampusId,
    int Capacity
);

public sealed record ConstraintData(
    int TeacherId,
    DateOnly Date,
    bool Hard
);

public sealed record RuleData(
    string Key,
    bool Enabled,
    string? Severity
);

public sealed record PlanningSnapshot(
    EventData[] Events,
    SessionData[] Sessions,
    RoomData[] Rooms,
    Dictionary<DateOnly, bool> Days,
    ConstraintData[] Constraints,
    RuleData[] Rules,
    string[] Acknowledged,
    int[] VirtualCampusIds
);