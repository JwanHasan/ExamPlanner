namespace DatabaseConnection.model;
public class RuleSetting
{
	public int Id { get; set; }
	public bool Enable { get; set; }
	public required string Servierty { get; set; }

    public ICollection<ConflictAcknowledgement>? ConflictAcknowledgements{get;set;}
    
}
