namespace DatabaseConnection.model;

public sealed class ScheduleEditAudit
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BeforeJson { get; set; } = "[]";
    public string AfterJson { get; set; } = "[]";
}