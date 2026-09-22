using System.Dynamic;

namespace DatabaseConnection.model;
public class StudentExamAssignment
{
    public int Id {get;set;}
    public required bool ExtraTime{get;set;}
    public required Student Student{get;set;}
    public required Exam Exam{get;set;}
}