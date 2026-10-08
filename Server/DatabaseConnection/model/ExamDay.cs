using System.Dynamic;
namespace DatabaseConnection.model;

public enum ExamDayType{Ordinary,ReExam}
public class ExamDay
{
    public int Id{get;set;}
    public int ScheduleId{get;set;}
    public DateTime Date {get;set;}
    public ExamDayType ExamDayType {get;set;}
    public bool Usable{get;set;} = false;

    public Schedule Schedule{get;set;} = new Schedule{};
}