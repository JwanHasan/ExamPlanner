namespace DatabaseConnection.model;

public class Enrollment
{
    public required int StudentId {get;set;} // PPK, FK
    public required int ClassId {get;set;} //PPK,FK
  

    // relation Enrollment must have 1 Class and Class can have 0 or many Enrollment

    public required Class Class{get;set;}
    // Enrollment has 1 student and student have many enrollment 
    public required Student Student{get;set;}
}