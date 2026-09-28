namespace DatabaseConnection.model;

public enum UserRole
{
    Admin,
    Teacher,
    Student
}
public class UserAccount{
    public int Id{get;set;}
    public required string Email {get;set;}
    public required string PasswordHash{get;set;}
    public UserRole userRole{get;set;}
    public Student? StudentAccount {get;set;}
    public Lecturer? LecturerAccount{get;set;}
}