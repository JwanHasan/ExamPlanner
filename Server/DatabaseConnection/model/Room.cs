namespace DatabaseConnection.model;
public class Room
{
    public int Id{get;set;}
    public string RoomCode{get;set;}= "";
    public string Campus {get;set;} = "";
    public int Capacity{get;set;}

    public ICollection<ExamSessionRoom>? ExamSessionRooms {get;set;}
}