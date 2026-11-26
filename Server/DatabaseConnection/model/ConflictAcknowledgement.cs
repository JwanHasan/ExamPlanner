namespace DatabaseConnection.model;

public class ConflictAcknowledgement
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public int RuleKeyId { get; set; }
    public int UserId { get; set; }
    public string FingerPrint { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = "";
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Schedule Schedule{get;set;}= null!;
    public RuleSetting RuleSetting {get;set;} = null!;
    public User User{get;set;} =null!;
}

