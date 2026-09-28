namespace DatabaseConnection.model;
public class Class
{
    public int Id{get;set;} //PK
    public int CourseId{get;set;} //FK
    public string ClassCode{get;set;} = ""; //CK

    public string NickName{get;set;}="";
    public string Prefix {get;set;}="";
    public int Semester {get;set;}
    public DateTime StartDate{get;set;}
    public DateTime EndDate{get;set;}
    public string CourseOffering {get;set;}="";



// relation class can have 1 course and course can have many classes 

public Course Course {get;set;} = new Course();
// relation enrollment has 1 class and class has 0 or  many enrollment
public ICollection<Enrollment>? Enrollments {get;set;}

public ICollection<ExamClass>? ExamClasses {get;set;}


}
