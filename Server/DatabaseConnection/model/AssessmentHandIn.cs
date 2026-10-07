namespace DatabaseConnection.model;
public class AssessmentHandIn
{
    public int Id{get;set;}
    public int AssessmentEventId{get;set;}
    public DateTime HandInDate{get;set;}
    public int Part {get;set;}
    public string Note{get;set;} ="";

    public AssessmentEvent AssessmentEvent {get;set;} = new AssessmentEvent{};
    
}