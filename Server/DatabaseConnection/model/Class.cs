namespace DatabaseConnection.model;
public class Class
{
    public int ClassID{get;set;} //PK
    public required int CourseId{get;set;} //FK
    public required string ClassCode{get;set;} //CK

    public required string NickName{get;set;}
    public required string Prefix {get;set;}
    public required int Semester {get;set;}
    public DateTime StartDate{get;set;}
    public DateTime EndDate{get;set;}
    public required string ClassOffering {get;set;}



// relation class can have 1 course and course can have many classes 

public required Course Course {get;set;}
// relation enrollment has 1 class and class has 0 or  many enrollment
public ICollection<Enrollment> Enrollments {get;set;} = new List<Enrollment>();

public ICollection<ExamClass> ExamClasses {get;set;} = new List<ExamClass>();


}
