namespace DatabaseConnection.model;

public class ConflictAcknowledgement
{
    public string Id { get; set; } = string.Empty;
    public string ScheduleId { get; set; } = string.Empty;
    public string RuleKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FingerPrint { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Schedule Schedule{get;set;}= null!;
    public RuleSetting RuleSetting {get;set;} = null!;


}

