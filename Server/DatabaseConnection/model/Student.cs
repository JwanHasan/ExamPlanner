using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace DatabaseConnection.model;
public class Student
{
    public int Id {get;set;}
    public int UserId{get;set;}

    public ICollection<Enrollment>? Enrollments {get;set;}
    
    public User User {get;set;} = new User{};
}