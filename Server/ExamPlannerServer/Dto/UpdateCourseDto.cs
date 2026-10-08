namespace ExamPlannerServer.Dto;

public class UpdateCourseDto
{
    public int ClassCount {get; set;}
    public required string Name {get;set;}
    public required string SueCode {get;set;}
    public required string? Notes {get; set;}
}