namespace DatabaseConnection.model;
public class PlanningEducationElement
{
    public enum Assessment
    {
        Internal,External
    }
    public int Id{get;set;}
    public string SourceChecksum{get;set;}= "";
    public DateTime SourceModifiedAt{get;set;}
    public string Name{get;set;}= "";
    public string Nickmane{get;set;} = "";
    public int ECTS {get;set;}
    public int ExecutionElementCount{get;set;}
    public DateOnly StartDate{get;set;}
    public DateOnly EndDate{get;set;}
    public string ActivityOffering{get;set;}= "";
    public string Prefix{get;set;}= "";
    public Assessment AssessmenType {get;set;}
    public bool CompletionAssesment{get;set;}
    public string GradingScale{get;set;} = "7-Grading Scale";
    public bool CombinedWrittenOral{get;set;}
    public bool Oral{get;set;}
    public bool PracticalExam{get;set;}
    public bool Project{get;set;}
    public bool Written{get;set;}

    public AssessmentEvent AssessmentEvent{get;set;} = new();
    
}