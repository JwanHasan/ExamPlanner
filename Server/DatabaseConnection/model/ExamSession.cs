namespace DatabaseConnection.model;

public class ExamSession
{
    public enum Sessions
    {
        Ordinary, ReExam
    }

    public required int ExamSessionId{get;set;} //PK
    public required int ExamId {get;set;}//FK
    public DateTime ExamDate{get;set;}
    public Sessions SessionType {get;set;}

    public string? Note {get;set;}

    // exam session has 1 Exam and exam can have 0 or many Exam Session
    public required Exam Exam{get;set;}
}