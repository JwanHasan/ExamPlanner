namespace DatabaseConnection.model;

public class Course
{
    public int Id {get;set;} //PK
    public string SueCode {get;set;}="";
    public string Name {get;set;} ="";
    public int ECTS {get;set;}

    public string? Notes {get; set;}

    //relation 1 to 0..* between course and class

    public ICollection<Class> Classes {get;set;}= new List<Class>();
}