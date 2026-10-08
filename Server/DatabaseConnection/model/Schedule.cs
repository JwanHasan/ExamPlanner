namespace DatabaseConnection.model;
public class Schedule
{
    public int Id {get;set;}
    public bool Approved {get;set;} = false;
    public string Version {get;set;}= "";
    public DateTime CreatedAt = DateTime.Now;

    
}