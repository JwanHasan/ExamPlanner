namespace ExamPlannerServer.Dto;
public class UpdateStudentDto
{
    public int Id {get;set;}
    public required string Name {get;set;}
    public required int ViaId{get;set;}
}