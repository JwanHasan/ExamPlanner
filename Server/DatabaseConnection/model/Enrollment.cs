using System.Diagnostics.CodeAnalysis;

namespace DatabaseConnection.model;

public class Enrollment
{
    public required int StudentId {get;set;} // PPK, FK
    public required int CourseId {get;set;} //PPK,FK
  

    // relation Enrollment must have 1 Class and Class can have 0 or many Enrollment

    public  Course Course {get;set;} = new Course{};
    // Enrollment has 1 student and student have many enrollment 
    public Student Student {get;set;} = new Student{};
}