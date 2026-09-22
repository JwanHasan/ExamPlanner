namespace DatabaseConnection.model;
public class ExamSessionRoom
{
    public int ExamSessionId{get;set;}
    public int RoomId{get;set;}

    public required ExamSession ExamSession{get;set;}
    public required Room Room{get;set;}
}