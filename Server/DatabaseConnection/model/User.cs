using System.Dynamic;
namespace DatabaseConnection.model;
public enum Role{
    Teacher,Student
}
public class User
{
    public int Id{get;set;}
    public string Email{get;set;} = "";
    public string Name {get;set;} = "";
    public string HashedPassword {get;set;} = "";
    public Role Role{get;set;}

    public Student? Student {get;set;}
    public Teacher? Teacher{get;set;}
}