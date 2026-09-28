namespace ExamPlannerServer.Dto;

public class UpdateCourseDto
{
    public string? Notes { get; set; }
    public int ClassCount { get; set; }
    public required string Name{get;set;}
    public required string SueCode {get;set;}
    
}