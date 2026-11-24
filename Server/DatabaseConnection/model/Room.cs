using System.Dynamic;
using System.Security.Claims;

namespace DatabaseConnection.model;
public class Room
{
    public int Id{get;set;}
    public int CampusId{get;set;}
    public string Name{get;set;} = "";
    public int Capacity{get;set;}
    
    public ICollection<ExamSession>? ExamSessions {get;set;}
    public Campus Campus {get;set;} =null!;
}