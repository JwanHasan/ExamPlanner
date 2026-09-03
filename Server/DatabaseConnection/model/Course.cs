public class Course
{
    public int CourseId {get;set;} //PK
    public string SueCode {get;set;}="";
    public required string Name {get;set;}
    public int Ects {get;set;}
    

    //relation 1 to 0..* between course and class

    public ICollection<Class> Classes {get;set;}= new List<Class>();
}