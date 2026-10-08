namespace DatabaseConnection.model;

public class Course
{
    public int Id {get;set;} //PK
    public string SueCode {get;set;}="";
    public string Prefix {get;set;} = "";
    public string Name {get;set;} ="";
    public int Semester {get;set;}
    public int ETCS {get;set;}
    public int PriorityTier{get;set;}
    

    //relation 1 to 0..* between course and class

    public ICollection<Class>? Classes {get;set;}
    public ICollection<Enrollment>? Enrollments {get;set;}
    public ICollection<AssessmentEvent>? AssessmentEvents {get;set;}
    
}