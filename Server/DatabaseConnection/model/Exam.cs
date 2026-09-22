namespace DatabaseConnection.model;

public class Exam
{
   public enum Examiner
    {
        Internal,External
    }
    
    public int Id {get;set;} //PK
    public string GradingScale{get;set;} = "7 grading Scale";
    public Examiner ExaminerType {get;set;}

    public required string ExamFormat{get;set;}

    public  string Prerequisites {get;set;} = "";

    public required string Duration {get;set;}
    public required string PlanningResponsible {get;set;}
    public required string City{get;set;}
    public string Remarks {get;set;} = "";

    // relation Exam Class has 1 Exam and exam has 1 or many exam classes
    public ICollection<ExamClass>? ExamClasses {get;set;}

    // exam scheduled as exam session and can have 0 or many and exam session can only have 1 exam

    public ICollection<ExamSession>? ExamSessions{get;set;}

    // exam hand in has only 1 exam and exam can have 0 or many hand in


    public ICollection<ExamHandIn>? ExamHandIns{get;set;}

    // exam lecture in has only 1 exam and exam can have 0 or many exam lecture

    public ICollection<ExamLecturer>? ExamLecturers{get;set;}

    public ICollection<StudentExamAssignment>? StudentExamAssignments {get;set;}
}