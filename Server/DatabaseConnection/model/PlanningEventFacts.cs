namespace DatabaseConnection.model;

public sealed class PlanningEventFacts
{
    public int AssessmentEventId { get; set; }
    public AssessmentEvent AssessmentEvent { get; set; } = null!;
    public bool? CoTaught { get; set; }
    public bool? SmallSemester { get; set; }
    public bool? GraduatingStudents { get; set; }
    public bool? LargeClass { get; set; }
    public string HoldsJson { get; set; } = "[]";
    public string AvoidWeekdaysJson { get; set; } = "[]";
    public string AvoidDatesJson { get; set; } = "[]";
}