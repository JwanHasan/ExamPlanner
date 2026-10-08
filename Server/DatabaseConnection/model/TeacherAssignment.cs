namespace DatabaseConnection.model;

public enum TeacherRole
{
    Internal,External
}
public class TeacherAssignment
{
    public int ExamSessionId {get;set;}
    public int TeacherId{get;set;}
    public TeacherRole Role{get;set;}

    public Teacher Teacher{get;set;}= new Teacher();
    public ExamSession ExamSession {get;set;} = new ExamSession();
}