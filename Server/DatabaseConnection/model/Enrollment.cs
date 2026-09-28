namespace DatabaseConnection.model;

public class Enrollment
{
    public required int StudentId {get;set;} // PPK, FK
    public required int ClassId {get;set;} //PPK,FK
  

    // relation Enrollment must have 1 Class and Class can have 0 or many Enrollment

    public  Class Class{get;set;} = new Class();
    // Enrollment has 1 student and student have many enrollment 
    public  Student Student{get;set;} = new Student();
}