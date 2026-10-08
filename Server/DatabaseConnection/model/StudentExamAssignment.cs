namespace DatabaseConnection.model;

public class StudentExamAssignment
{
    public int StudentId{get;set;}
    public int ExamSessionId{get;set;}
    public bool ExtraTime{get;set;}

    public Student Student{get;set;} = new Student();
    public ExamSession ExamSession {get;set;} = new ExamSession();
}