namespace DatabaseConnection.model;

public class Campus
{
    public int Id{get;set;}
    public string Name{get;set;}= string.Empty;
    public bool IsVirtual{get;set;}

    public ICollection<PlanningEducationElement>? PlanningEducationElements{get;set;}
    public ICollection<Room>? Rooms{get;set;}
    public ICollection<ExamSession>? ExamSessions{get;set;}


}