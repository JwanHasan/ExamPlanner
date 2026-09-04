using System.Dynamic;

namespace DatabaseConnection.model;
public class Student
{
    public int Id {get;set;} //Pk
    public  string Name {get;set;} = "No name provided";


    // student has 0 or many enrollment and enrollment can have only 1 student
    public ICollection<Enrollment> Enrollments {get;set;} = new List<Enrollment>();
}