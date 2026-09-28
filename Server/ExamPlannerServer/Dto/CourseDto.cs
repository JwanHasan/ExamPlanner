namespace ExamPlannerServer.Dto;

public class CourseDto
{
    public required int Id{get;set;}
    public required string Name{get;set;}
    public required string Semester{get;set;}
    public required string SueCode {get;set;}
    public required int Etcs {get;set;}
    public string? Notes { get; set; }
    public int ClassCount { get; set; }
    
}