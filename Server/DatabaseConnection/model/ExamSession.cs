namespace DatabaseConnection.model;

public class ExamSession
{
    public int Id { get; set; }
    public int AsseessmenEventId { get; set; }
    public int ScheduleId { get; set; }
    public int? RoomId { get; set; }
    public int CampusId { get; set; }
    public DateOnly Date { get; set; }
    public bool LockDate { get; set; }
    public string GroupLabel { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string RowVersion { get; set; } = string.Empty;
    public Room? Room { get; set; }

    public ICollection<StudentExamAssignment>? StudentExamAssignments { get; set; }
    public ICollection<TeacherAssignment>? TeacherAssignments { get; set; }
    public AssessmentEvent AssessmentEvent { get; set; } = new();
    public Schedule Schedule { get; set; } = new();
    public Campus? Campus { get; set; }
}