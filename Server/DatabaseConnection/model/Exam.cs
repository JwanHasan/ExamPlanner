using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

public class Exam
{
   public enum Examiner
    {
        Internal,External
    }
    public int ExamId {get;set;} //Pk
    public int GradingScale{get;set;}
    public Examiner ExaminerType {get;set;}

    public required string ExamFormat{get;set;}

    public required string Prerequisites {get;set;}

    public required string Duration {get;set;}
    public required string PlanningResponsible {get;set;}
    public required string City{get;set;}
    public string Remarks {get;set;} = "";

}