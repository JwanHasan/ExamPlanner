namespace DatabaseConnection.model;
public enum ExamType
{
    Oral,Written,Sep
}
public class AssessmentEvent
{
    public int Id{get;set;}
    public int CourseId{get;set;}
    public int PlanningElementId{get;set;}
    public ExamType ExamType {get;set;}
    public string Note {get;set;}= "";

    public ICollection<ExamSession>? ExamSessions{get;set;}
    public ICollection<AssessmentHandIn>? AssessmentHandIns{get;set;}
    public Course? Course {get;set;}

}