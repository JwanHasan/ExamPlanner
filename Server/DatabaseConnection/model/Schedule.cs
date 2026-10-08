namespace DatabaseConnection.model;
public class Schedule
{
    public int Id {get;set;}
    public bool Approved {get;set;} = false;
    public string Version {get;set;}= "";
    public DateTime CreatedAt = DateTime.Now;

    public ICollection<ExamSession>? ExamSessions{get;set;}
    public ICollection<ScheduleReview>? ScheduleReviews{get;set;}
    public ICollection<ExamDay> ExamDays{get;set;} = new List<ExamDay>();

    
}