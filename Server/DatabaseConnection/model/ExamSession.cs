namespace DatabaseConnection.model;
public enum SessionType
    {
        Ordinary, ReExam
    };
public class ExamSession
{
    public required int Id{get;set;} //PK
    public required int ExamId {get;set;}//FK
    public DateTime ExamDate{get;set;}
    public SessionType SessionType {get;set;}

    public string? Note {get;set;}

    public bool Locked{get;set;} = false;

    // exam session has 1 Exam and exam can have 0 or many Exam Session
    public required Exam Exam{get;set;}
    public required Schedule Schedule{get;set;}

}