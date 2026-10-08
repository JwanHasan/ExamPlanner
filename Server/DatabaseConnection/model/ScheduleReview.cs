namespace DatabaseConnection.model;
public class ScheduleReview
{
    public int ScheduleId{get;set;}
    public int TeacherId{get;set;}
    public bool Approved{get;set;}= false;
    public bool Completed{get;set;}= false;
    public string Remarks{get;set;}="";

    public Schedule Schedule {get;set;}= new();
    public Teacher Teacher {get;set;}= new();
}