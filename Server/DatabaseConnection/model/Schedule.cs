using DatabaseConnection.model;

public class Schedule
{
    public int Id{get;set;}
    public bool Approved{get;set;}
    public string version {get;set;}= "";
    public DateTime CreatedAt {get;set;}

    public required ICollection<ExamDay> ExamDays{get;set;}
    public ICollection<ExamSession>? ExamSessions{get;set;}
}
