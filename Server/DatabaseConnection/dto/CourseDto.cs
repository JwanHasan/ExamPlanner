namespace DatabaseConnection.dto;
public class CourseDto
{
    public string SueCode {get;set;}="";
    public required string Name {get;set;}
    public int ECTS {get;set;}
    public string? Notes {get;set;}
}