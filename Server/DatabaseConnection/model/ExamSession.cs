namespace DatabaseConnection.model;
public class ExamSession
{
    public int Id{get;set;}
    public int AsseessmentId{get;set;}
    public int ScheduleId{get;set;}
    public int RoomId{get;set;}
    public DateOnly Date{get;set;}
    public bool LockDate{get;set;}

    public Room Room {get;set;} = new Room{}; 
}