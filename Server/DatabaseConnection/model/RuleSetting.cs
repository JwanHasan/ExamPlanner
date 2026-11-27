namespace DatabaseConnection.model;
public class RuleSetting
{
	public int Id { get; set; }
	public bool Enabled { get; set; }
	public required string Severity { get; set; }

    public ICollection<ConflictAcknowledgement>? ConflictAcknowledgements{get;set;}
    
}
