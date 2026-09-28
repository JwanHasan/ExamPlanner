namespace ExamPlannerServer.Dto;

public class CreateNewStudentDto
{
    public required string Name {get;set;}
    public required int ViaId{get;set;}
    public int UserAccountId{get;set;}
}