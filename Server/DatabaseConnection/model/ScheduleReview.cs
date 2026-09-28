using System.Data.Common;

namespace DatabaseConnection.model;
public class ScheduleReview
{
    public int ScheduleId{get;set;}
    public int LecturerId{get;set;}
    public bool Approved {get;set;}
    public bool Completed{get;set;}
    public string Remarks {get;set;}= "";

    public required Lecturer Lecturer{get;set;}
    public required Schedule Schedule{get;set;}
}