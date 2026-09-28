using System.Dynamic;

namespace DatabaseConnection.model;
public enum ExamDayType{Ordinal,ReExam};
public class ExamDay
{
    public int Id {get;set;}
    public int ScheduleId{get;set;}
    public DateTime Date {get;set;}
    public ExamDayType ExaminerType{get;set;}
    public bool Useable {get;set;}

    public required Schedule Schedule{get;set;}

}