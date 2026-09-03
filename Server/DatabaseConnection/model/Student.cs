using System.Diagnostics.Contracts;

public class Student
{
    public int StudentId {get;set;} //Pk


    // student has 0 or many enrollment and enrollment can have only 1 student
    public ICollection<Enrollment> Enrollments {get;set;} = new List<Enrollment>();
}